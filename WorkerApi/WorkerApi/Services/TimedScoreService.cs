
namespace WorkerApi.Services
{
    public class TimedScoreService : BackgroundService
    {
        private ICachedScorerService _scorer;

        public TimedScoreService(ICachedScorerService scorer) {
            _scorer = scorer;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    _scorer.ComputeScore();
                }
                catch (Exception ex) { }
                await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
            }
        }
    }
}
