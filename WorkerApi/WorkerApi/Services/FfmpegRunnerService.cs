using WorkerApi.Models;
using System.Diagnostics;
using WorkerApi.Services.Process;

namespace WorkerApi.Services
{
    public class FfmpegRunnerService
    {
        private IProcessWrapper _ffmpegProcess;
        private readonly ILogger<FfmpegRunnerService> _logger;
        private readonly IProcessFactory _processFactory;

        public FfmpegRunnerService(ILogger<FfmpegRunnerService> logger, IProcessFactory processFactory)
        {
            _logger = logger;
            _processFactory = processFactory;
        }

        public async Task RunFfmpegCommandAsync(VideoCommand ffmpegCommand)
        {
            // Arrêter tous les processus ffmpeg existants
            StopAllFfmpegProcesses();

            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = ffmpegCommand.Compile(),
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _ffmpegProcess = _processFactory.CreateProcess();
            _ffmpegProcess.StartInfo = startInfo;
            _ffmpegProcess.EnableRaisingEvents = true;

            // Gestions des sorties standard et d'erreur
            _ffmpegProcess.OutputDataReceived += (sender, e) => _logger.LogInformation($"[Ffmpeg Output] {e.Data}");
            _ffmpegProcess.ErrorDataReceived += (sender, e) => _logger.LogError($"[Ffmpeg Error] {e.Data}");

            try
            {
                // Démarrage du processus FFmpeg
                _ffmpegProcess.Start();
                _ffmpegProcess.BeginOutputReadLine();
                _ffmpegProcess.BeginErrorReadLine();
                _logger.LogInformation("FFmpeg process started");
                await _ffmpegProcess.WaitForExitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running FFmpeg command");
            }
        }

        public void StopAllFfmpegProcesses()
        {
            var ffmpegProcesses = System.Diagnostics.Process.GetProcessesByName("ffmpeg");

            foreach (var process in ffmpegProcesses)
            {
                try
                {
                    process.Kill();
                    process.Dispose();
                    _logger.LogInformation($"FFmpeg process {process.Id} stopped");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error stopping FFmpeg process {process.Id} : {ex.Message}");
                }
            }
        }
    }
}
