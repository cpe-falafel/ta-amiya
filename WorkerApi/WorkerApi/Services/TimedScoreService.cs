
namespace WorkerApi.Services
{
    public class TimedScoreService : BackgroundService
    {
        private ICachedScorerService _scorer;
        private IZmqCommandService _zmqService;

        public TimedScoreService(ICachedScorerService scorer, IZmqCommandService zmqService) {
            _scorer = scorer;
            _zmqService = zmqService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await _scorer.ComputeScore();
                    _ = _zmqService.SendCommandAsync(_scorer.GetCachedScore() > _scorer.GetCachedMinScore());
                }
                catch (Exception ex) { }
                await Task.Delay(TimeSpan.FromSeconds(20), stoppingToken);
            }
        }
    }
}
