
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
            using var timer = new PeriodicTimer(TimeSpan.FromSeconds(1));
            while (await timer.WaitForNextTickAsync(stoppingToken))
            {
                await _scorer.ComputeScore();
                _zmqService.SendCommand(_scorer.GetCachedScore() > _scorer.GetCachedMinScore());
            }
        }
    }
}
