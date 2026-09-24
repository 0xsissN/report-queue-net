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
                job.Status = JobStatus.Failed;
                job.Error = error;
                job.CompletedAt = DateTime.UtcNow;
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
    }
}