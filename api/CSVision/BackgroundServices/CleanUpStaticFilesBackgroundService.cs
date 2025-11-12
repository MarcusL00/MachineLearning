namespace CSVision.BackgroundServices
{
    public sealed class CleanUpStaticFilesBackgroundService : BackgroundService
    {
        private readonly IServiceProvider _serviceProvider;
        private readonly TimeSpan _jobInterval;

        public CleanUpStaticFilesBackgroundService(
            IServiceProvider serviceProvider,
            TimeSpan jobInterval
        )
        {
            _serviceProvider = serviceProvider;
            _jobInterval = jobInterval;
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
        {
            return Task.CompletedTask;
        }
    }
}
