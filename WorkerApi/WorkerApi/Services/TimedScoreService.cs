
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
            _ = Task.Run(async () =>
            {
                using var timer = new PeriodicTimer(TimeSpan.FromSeconds(10));
                while (await timer.WaitForNextTickAsync())
                {
                    await _scorer.ComputeScore();
                    _zmqService.SendCommand(_scorer.GetCachedScore() > _scorer.GetCachedMinScore());
                }
            }, stoppingToken);
        }
    }
}
