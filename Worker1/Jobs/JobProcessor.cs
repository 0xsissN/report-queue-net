using Domain.Entities;

namespace Worker1.Jobs
{
    public class JobProcessor
    {
        public async Task ProcessAsync(Job job, CancellationToken cancellationToken)
        {
            Console.WriteLine($"Processing job {job.Id}, attempt {job.Attempts}");

            await Task.Delay(1000, cancellationToken);

            throw new Exception("Simulated processing failure");
        }
    }
}