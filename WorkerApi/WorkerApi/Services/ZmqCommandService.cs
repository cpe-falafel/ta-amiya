using NetMQ;
using NetMQ.Sockets;

namespace WorkerApi.Services
{
    public class ZmqCommandService : IZmqCommandService
    {
        private readonly string _zmqServerAddress;
        private readonly ILogger<ZmqCommandService> _logger;

        public ZmqCommandService(ILogger<ZmqCommandService> logger, string zmqServerAddress = "tcp://localhost:5555")
        {
            _logger = logger;
            _zmqServerAddress = zmqServerAddress;
        }

        private async void Send(string msg)
        {
            using (var client = new RequestSocket())
            {
                client.Connect(_zmqServerAddress);
                client.SendFrame(msg);
                using (var cts = new CancellationTokenSource(TimeSpan.FromSeconds(10)))
                {
                    try
                    {
                        var response = await client.ReceiveFrameStringAsync(cts.Token);
                        _logger.LogInformation("FFMPEG sent: ", response.ToString());
                    }
                    catch (OperationCanceledException)
                    {
                        _logger.LogWarning("Aucune réponse de FFMPEG");
                    }
                }
            }
        }

        public async Task SendCommandAsync(bool applyBlur)
        {
            try
            {
                string command = applyBlur ? "gblur@censor enable 1" : "gblur@censor enable 0";

                _logger.LogInformation($"Sending command: {command}");

                Send(command);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending command to ZMQ server");
            }
        }
    }
}
