using Domain.Contracts;
using Domain.Entities;
using Domain.Enum;
using Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;

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
            var job = new Job
            {
                Id = Guid.NewGuid(),
                Type = request.Type,
                Payload = request.Payload,
                Status = JobStatus.Queued,
                Attempts = 0,
                MaxAttempts = 3,
                CreatedAt = DateTime.UtcNow,
                AvailableAt = DateTime.UtcNow,
            };

            _context.Job.Add(job);

            await _context.SaveChangesAsync(cancellationToken);

            return Ok(new
            {
                job.Id,
                job.Status
            });
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