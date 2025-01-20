using WorkerApi.Models;
using System.Diagnostics;
using WorkerApi.Services.Process;
using System.Text.Json;

namespace WorkerApi.Services
{
    public class FfmpegRunnerService
    {
        private IProcessWrapper _ffmpegProcess;
        private readonly ILogger<FfmpegRunnerService> _logger;
        private readonly IProcessFactory _processFactory;
        private readonly string _pidFilePath = "ffmpeg_pid.txt";

        public FfmpegRunnerService(ILogger<FfmpegRunnerService> logger, IProcessFactory processFactory)
        {
            _logger = logger;
            _processFactory = processFactory;
        }

        private async Task SaveProcessIdAsync(int pid)
        {
            await File.WriteAllTextAsync(_pidFilePath, pid.ToString());
            _logger.LogInformation($"FFmpeg process ID {pid} saved to file {_pidFilePath}");
        }

        private async Task<int?> LoadProcessListAsync()
        {
            if (!File.Exists(_pidFilePath))
            {
                return null;
            }

            var content = await File.ReadAllTextAsync(_pidFilePath);
            return int.TryParse(content, out int pid) ? pid : null;
        }

        private async Task ClearProcessIdAsync()
        {
            if (File.Exists(_pidFilePath))
            {
                File.Delete(_pidFilePath);
                _logger.LogInformation($"FFmpeg process ID file {_pidFilePath} deleted");
            }
        }

        public async Task RunFfmpegCommandAsync(VideoCommand ffmpegCommand)
        {
            // Vérification si un processus FFmpeg est déjà en cours et le stopper si c'est le cas
            await StopExistingProcessAsync();

            ProcessStartInfo startInfo = new ProcessStartInfo("ffmpeg", ffmpegCommand.Args)
            {
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
                await SaveProcessIdAsync(_ffmpegProcess.ProcessId);

                _ffmpegProcess.BeginOutputReadLine();
                _ffmpegProcess.BeginErrorReadLine();
                _logger.LogInformation("FFmpeg process started");

                await _ffmpegProcess.WaitForExitAsync();
                await ClearProcessIdAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running FFmpeg command");
                await ClearProcessIdAsync();
            }
        }

        public async Task StopExistingProcessAsync()
        {
            var pid = await LoadProcessListAsync();
            if (pid.HasValue)
            {
                try
                {
                    var process = System.Diagnostics.Process.GetProcessById(pid.Value);
                    if (process.ProcessName.ToLower() == "ffmpeg")
                    {
                        process.Kill();
                        process.Dispose();
                        _logger.LogInformation($"FFmpeg process {pid.Value} stopped");

                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error stopping FFmpeg process {pid.Value} : {ex.Message}");
                }
                finally
                {
                    await ClearProcessIdAsync();
                }
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
