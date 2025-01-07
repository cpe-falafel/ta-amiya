using System.Diagnostics;

namespace WorkerApi.Services
{
    public class FfmpegRunnerService
    {
        private Process _ffmpegProcess;
        private readonly ILogger<FfmpegRunnerService> _logger;

        public async Task RunFfmpegCommandAsync(string ffmpegCommand)
        {
            StopFfmpegCommand(); // Stop any running FFmpeg process

            _ffmpegProcess = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = "ffmpeg",
                    Arguments = ffmpegCommand,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };

            // Gestions des sorties standard et d'erreur
            _ffmpegProcess.OutputDataReceived += (sender, e) => _logger.LogInformation($"[Ffmpeg Output] {e.Data}");
            _ffmpegProcess.ErrorDataReceived += (sender, e) => _logger.LogError($"[Ffmpeg Error] {e.Data}");
            
            try
            {
                // Démarrage du processus FFmpeg
                _ffmpegProcess.Start();
                _ffmpegProcess.BeginOutputReadLine(); // Début de la lecture de la sortie standard
                _ffmpegProcess.BeginErrorReadLine(); // Début de la lecture de la sortie d'erreur

                _logger.LogInformation("FFmpeg process started");

                await _ffmpegProcess.WaitForExitAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running FFmpeg command");
            }
        }

        public void StopFfmpegCommand()
        {
            if (_ffmpegProcess != null && !_ffmpegProcess.HasExited)
            {
                try
                {
                    _ffmpegProcess.Kill(); // Kill the process
                    _ffmpegProcess.Dispose(); // Dispose the process to free resources
                    _logger.LogInformation("FFmpeg process stopped");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error stopping FFmpeg process: {ex.Message}");
                }
            }
        }
    }
}
