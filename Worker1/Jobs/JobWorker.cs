namespace Worker1.Jobs
{
    public class JobWorker: BackgroundService
    {
        private readonly JobProcessor _processor;
        public JobWorker(JobProcessor processor) => _processor = processor;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Console.WriteLine("Looking for jobs...");

            await _processor.ProcessAsync(stoppingToken);

            await Task.Delay(1000, stoppingToken);
        }
    }
}
