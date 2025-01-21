using NetMQ;
using NetMQ.Sockets;

namespace WorkerApi.Services
{
    public class ZmqCommandService : IZmqCommandService
    {
        private readonly string _zmqServerAddress;
        private readonly ILogger<ZmqCommandService> _logger;
        private readonly RequestSocket _socket;

        public ZmqCommandService(ILogger<ZmqCommandService> logger, string zmqServerAddress = "tcp://localhost:5555")
        {
            _logger = logger;
            _zmqServerAddress = zmqServerAddress;
            _socket = new RequestSocket();
            _socket.Connect(_zmqServerAddress); 
        }

        public async Task<string> SendCommandAsync(bool applyBlur)
        {
            try
            {
                string command = applyBlur ? "gblur@censor enable 1" : "gblur@censor enable 0";

                _logger.LogInformation($"Sending command: {command}");

                await Task.Run(() => _socket.SendFrame(command));

                var response = await Task.Run(() => _socket.ReceiveFrameString());

                _logger.LogInformation($"Received response: {response}");

                return response;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error sending command to ZMQ server");
                return string.Empty;
            }
        }
    }
}
