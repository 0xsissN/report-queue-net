using Domain.Entities;
using Domain.Enum;
using Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Worker1.Jobs
{
    public class JobClaimer
    {
        private readonly DataContext _context;
        public JobClaimer(DataContext context) => _context = context;
        public async Task<Job?> ClaimAsync(string workerId, CancellationToken cancellationToken)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync(cancellationToken);

            var job = await _context.Job
                .FromSqlRaw("""
                    SELECT TOP (1) *
                    FROM [Job] WITH (UPDLOCK, READPAST, ROWLOCK)
                    WHERE [Status] = 0 AND ([AvailableAt] IS NULL OR [AvailableAt] <= SYSUTCDATETIME())
                    ORDER BY [CreatedAt]
                """)
                .FirstOrDefaultAsync(cancellationToken);

            if (job is null) return null;

            job.Status = JobStatus.Processing;
            job.StartedAt = DateTime.UtcNow;
            job.LockedAt = DateTime.UtcNow;
            job.LockedBy = workerId;
            job.Attempts++;

            await _context.SaveChangesAsync(cancellationToken);

            await transaction.CommitAsync(cancellationToken);

            return job;
        }
        public async Task RetryAsync(Job job, string error, CancellationToken cancellationToken)
        {
            if (job.Attempts >= job.MaxAttempts)
            {
                job.Status = JobStatus.DeadLetter;
                job.Error = error;
                job.CompletedAt = DateTime.UtcNow;
                job.AvailableAt = null;
            }
            else
            {
                var dailySecond = Math.Pow(2, job.Attempts);

                job.Status = JobStatus.Queued;
                job.Error = error;
                job.AvailableAt = DateTime.UtcNow.AddSeconds(dailySecond);
            }

            await _context.SaveChangesAsync(cancellationToken);
        }
        public async Task<int> RecoverStaleJobsAsync(CancellationToken cancellationToken) 
        {
            var timeout = DateTime.UtcNow.AddSeconds(-30);

            var jobs = await _context.Job
                .Where(x =>
                    x.Status == JobStatus.Processing &&
                    x.LockedAt != null &&
                    x.LockedAt < timeout
                ).ToListAsync(cancellationToken);
            
            foreach(var job in jobs)
            {
                job.Status = JobStatus.Queued;
                job.AvailableAt = DateTime.UtcNow;
                job.LockedAt = null;
                job.LockedBy = null;
                job.StartedAt = null;
            }

            await _context.SaveChangesAsync(cancellationToken);

            return jobs.Count;
        }
    }
}