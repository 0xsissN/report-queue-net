using Domain.Enum;

namespace Domain.Entities
{
    public class Job
    {
        public Guid Id { get; set; }
        public string Type { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public JobStatus Status { get; set; }
        public int Attempts { get; set; }
        public int MaxAttempts { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? StartedAt { get; set; }
        public DateTime? CompletedAt { get; set; }
        public DateTime? AvailableAt { get; set; }
        public DateTime? LockedAt { get; set; }
        public string? LockedBy { get; set; }
        public string? Error { get; set; }
    }
}
