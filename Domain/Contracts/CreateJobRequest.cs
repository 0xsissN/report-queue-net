namespace Domain.Contracts
{
    public class CreateJobRequest
    {
        public string Type { get; set; } = null!;
        public string Payload { get; set; } = null!;
        public string? IdempotencyKey { get; set; }
    }
}