using Domain.Data;
using Domain.Entities;
using Domain.Enum;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Text;

namespace Worker1.Jobs
{
    public class JobClaimer
    {
        private readonly DataContext _context;
        public JobClaimer(DataContext context) => _context = context;
        public async Task<Job?> ClaimAsync(string workerId, CancellationToken cancellationToken)
        {
            var job = await _context.Job
                .Where(x => x.Status == JobStatus.Queued)
                .OrderBy(x => x.CreatedAt)
                .FirstOrDefaultAsync(cancellationToken);

            if (job is null) return null;

            job.Status = JobStatus.Processing;
            job.StartedAt = DateTime.UtcNow;
            job.LockedAt = DateTime.UtcNow;
            job.LockedBy = workerId;
            job.Attempts++;

            await _context.SaveChangesAsync(cancellationToken);

            return job;
        }
    }
}
