using Worker1.Jobs;

namespace Worker1.Recovery
{
    public class JobRecoveryWorker: BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        public JobRecoveryWorker(IServiceScopeFactory scopeFactory) => _scopeFactory = scopeFactory;
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while(!stoppingToken.IsCancellationRequested)
            {
                using var scope = _scopeFactory.CreateScope();

                var claimer = scope.ServiceProvider.GetRequiredService<JobClaimer>();

                await claimer.RecoverStaleJobsAsync(stoppingToken);

                await Task.Delay(10000, stoppingToken);
            }
        }
    }
}
