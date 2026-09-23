namespace Worker1.Jobs
{
    public class JobWorker: BackgroundService
    {
        private readonly JobProcessor _processor;
        private readonly JobClaimer _claimer;
        public JobWorker(JobProcessor processor, JobClaimer claimer) 
        { 
            _processor = processor;
            _claimer = claimer;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            var workerId = Guid.NewGuid().ToString();

            while(!stoppingToken.IsCancellationRequested)
            {
                var job = await _claimer.ClaimAsync(workerId, stoppingToken);
                
                if(job is null)
                {
                    await Task.Delay(1000, stoppingToken);
                    continue;
                }

                await _processor.ProcessAsync(job, stoppingToken);
            }
        }
    }
}
