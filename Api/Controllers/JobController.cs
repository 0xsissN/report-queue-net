using Domain.Contracts;
using Domain.Entities;
using Domain.Enum;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace Api.Controllers
{
    [Route("api/jobs")]
    [ApiController]
    public class JobController : ControllerBase
    {
        private readonly DataContext _context;
        public JobController(DataContext context) => _context = context;

        [HttpPost]
        public async Task<IActionResult> Create(CreateJobRequest request, CancellationToken cancellationToken)
        {
            var existingJob = await _context.Job.FirstOrDefaultAsync(x => x.IdempotencyKey == request.IdempotencyKey);

            if (existingJob is not null) return Ok(existingJob);

            var job = new Job
            {
                Id = Guid.NewGuid(),
                Type = request.Type,
                Payload = request.Payload,
                IdempotencyKey = request.IdempotencyKey,
                Status = JobStatus.Queued,
                Attempts = 0,
                MaxAttempts = 3,
                CreatedAt = DateTime.UtcNow,
                AvailableAt = DateTime.UtcNow,
            };

            _context.Job.Add(job);

            try
            {
                await _context.SaveChangesAsync(cancellationToken);
            }
            catch(DbUpdateException)
            {
                var existing = await _context.Job.FirstAsync(x => x.IdempotencyKey == request.IdempotencyKey);

                return Ok(existing);
            }

            return Ok(job);
        }

        [HttpGet("{id:guid}")]
        public async Task<IActionResult> GetJobById(Guid id, CancellationToken cancellationToken)
        {
            var job = await _context.Job.FindAsync([id], cancellationToken);
            if (job is null) return NotFound();

            return Ok(new
            {
                job.Id,
                job.Type,
                job.Status,
                job.Attempts,
                job.CreatedAt,
                job.StartedAt,
                job.CompletedAt,
                job.Error
            });
        }
    }
}