namespace Worker1.Jobs
{
    public class JobProcessor
    {
        public async Task ProcessAsync(CancellationToken cancellationToken)
        {
            Console.WriteLine("Processing job...");

            await Task.Delay(1000, cancellationToken);

            Console.WriteLine("Job completed.");
        }
    }
}
