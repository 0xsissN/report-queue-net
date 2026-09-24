namespace Worker1.Jobs
{
    public class JobWorker : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public JobWorker(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var workerId = Guid.NewGuid().ToString();

            while (!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var claimer = scope.ServiceProvider.GetRequiredService<JobClaimer>();
                var processor = scope.ServiceProvider.GetRequiredService<JobProcessor>();

                var job = await claimer.ClaimAsync(workerId, stoppingToken);

                if (job is null)
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                try
                {
                    await processor.ProcessAsync(job, stoppingToken);
                }
                catch(Exception ex)
                {
                    Console.WriteLine($"Job {job.Id} failed: {ex.Message}");
                    await claimer.RetryAsync(job, ex.Message, stoppingToken);
                }

                Console.WriteLine($"-----------------------Worker {workerId} claimed job {job.Id}");
            }
        }
    }
}