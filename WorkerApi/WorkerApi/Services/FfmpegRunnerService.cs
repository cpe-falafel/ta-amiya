using WorkerApi.Models;
using System.Diagnostics;
using WorkerApi.Services.Process;
using Serilog;
using Serilog.Core;

namespace WorkerApi.Services
{
    public class FfmpegRunnerService
    {
        private IProcessWrapper _ffmpegProcess;
        private readonly ILogger<FfmpegRunnerService> _logger;
        private readonly IProcessFactory _processFactory;
        private Logger _ffmpegLogger;

        public FfmpegRunnerService(ILogger<FfmpegRunnerService> logger, IProcessFactory processFactory)
        {
            _logger = logger;
            _processFactory = processFactory;

            _ffmpegLogger = new LoggerConfiguration()
            .MinimumLevel.Verbose()
            .WriteTo.File(Path.Combine(Directory.GetCurrentDirectory(), "LogFiles", $"{DateTime.Now.Year}-{DateTime.Now.Month}-{DateTime.Now.Day}", "FfmpegLog.txt"),
                rollingInterval: RollingInterval.Infinite,
                outputTemplate: "[{Timestamp:yyyy-MM-dd HH:mm:ss.fff} [{Level}] {Message}{NewLine}{Exception}")
            .CreateLogger();
        }

        public async Task RunFfmpegCommandAsync(VideoCommand ffmpegCommand)
        {
            // Arrêter tous les processus ffmpeg existants
            StopAllFfmpegProcesses();

            ProcessStartInfo startInfo = new ProcessStartInfo("ffmpeg", ffmpegCommand.Args)
            {
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            };

            _logger.LogInformation("SENDING FFMPEG CMD " + string.Join(' ', startInfo.ArgumentList));

            _ffmpegProcess = _processFactory.CreateProcess();
            _ffmpegProcess.StartInfo = startInfo;
            _ffmpegProcess.EnableRaisingEvents = true;


            // Gestions des sorties standard et d'erreur
            _ffmpegProcess.OutputDataReceived += (sender, e) => _ffmpegLogger.Information("{@msg}", e.Data);
            _ffmpegProcess.ErrorDataReceived += (sender, e) => _ffmpegLogger.Error("{@msg}", e.Data);

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
                    process.WaitForExitAsync();
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
