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

        private void Send(string msg)
        {
            using (var client = new RequestSocket())
            {
                client.Connect(_zmqServerAddress);
                client.SendFrame(msg);
                client.TryReceiveFrameString(TimeSpan.FromSeconds(3) , out string response);
                _logger.LogInformation("FFMPEG sent: " + response);
            }
        }

        public void SendCommand(bool applyBlur)
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
