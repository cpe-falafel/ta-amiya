using WorkerApi.Models;
using System.Diagnostics;
using WorkerApi.Services.Process;
using Serilog;
using Serilog.Core;
using System.Text.Json;
using WorkerApi.Models.DTO;
using static NetMQ.NetMQSelector;
using System.Security.Cryptography;

namespace WorkerApi.Services
{
    public class FfmpegRunnerService
    {
        private IProcessWrapper _ffmpegProcess;
        private readonly ILogger<FfmpegRunnerService> _logger;
        private readonly IProcessFactory _processFactory;
        private Logger _ffmpegLogger;
        private readonly string _pidFilePath = "ffmpeg_pid.txt";

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
            // Vérification si un processus FFmpeg est déjà en cours et le stopper si c'est le cas

            try
            {
                bool stopped = await StopExistingProcessAsync();

                if (!stopped)
                {
                    _logger.LogWarning("Ffmeg could not stop process with register pid");
                    StopAllFfmpegProcesses();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error stopping existing FFmpeg process");
            }

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
                await SaveProcessIdAsync(_ffmpegProcess.ProcessId);

                _ffmpegProcess.BeginOutputReadLine();
                _ffmpegProcess.BeginErrorReadLine();
                _logger.LogInformation("FFmpeg process started");

                await _ffmpegProcess.WaitForExitAsync();
                ClearProcessId();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error running FFmpeg command");
                ClearProcessId();
            }
        }

        private async Task SaveProcessIdAsync(int pid)
        {
            await File.WriteAllTextAsync(_pidFilePath, pid.ToString());
            _logger.LogInformation($"FFmpeg process ID {pid} saved to file {_pidFilePath}");
        }

        private async Task<int?> LoadProcessIdAsync()
        {
            if (!File.Exists(_pidFilePath))
            {
                return null;
            }

            var content = await File.ReadAllTextAsync(_pidFilePath);
            return int.TryParse(content, out int pid) ? pid : null;
        }

        private void ClearProcessId()
        {
            if (File.Exists(_pidFilePath))
            {
                File.Delete(_pidFilePath);
                _logger.LogInformation($"FFmpeg process ID file {_pidFilePath} deleted");
            }
        }

        public async Task<bool> StopExistingProcessAsync()
        {
            var pid = await LoadProcessIdAsync();

            if (!pid.HasValue)
            {
                _logger.LogInformation("No FFmpeg process ID found to stop.");
                return false; // Aucun processus à arrêter
            }

            var process = pid.HasValue ? GetProcessById(pid.Value) : null;

            if (process != null)
            {
                try
                {
                    if (process.ProcessName.ToLower() == "ffmpeg")
                    {
                        process.Kill();

                        if (process.WaitForExit(TimeSpan.FromSeconds(20)))
                        {
                            _logger.LogInformation($"FFmpeg process {pid.Value} stopped");
                        }
                        else
                        {
                            _logger.LogWarning($"FFmpeg process {pid.Value} did not exit within the timeout period");
                        }

                        process.Dispose();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error stopping FFmpeg process {pid.Value} : {ex.Message}");
                }
                finally
                {
                    ClearProcessId();
                }
            }
            return GetProcessById(pid.Value) == null;
        }

        public void StopAllFfmpegProcesses()
        {
            var ffmpegProcesses = System.Diagnostics.Process.GetProcessesByName("ffmpeg");

            foreach (var process in ffmpegProcesses)
            {
                try
                {
                    process.Kill();

                    if (process.WaitForExit(TimeSpan.FromSeconds(20)))
                    {
                        _logger.LogInformation($"FFmpeg process {process.Id} stopped");
                    }
                    else
                    {
                        _logger.LogWarning($"FFmpeg process {process.Id} did not exit within the timeout period");
                    }

                    process.Dispose();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error stopping FFmpeg process {process.Id} : {ex.Message}");
                }
            }
        }

        private static System.Diagnostics.Process? GetProcessById(int processId)
        {
            try { 
                return System.Diagnostics.Process.GetProcessById(processId);
            } catch(ArgumentException)
            {
                return null;
            }
        }


        public async Task<WorkerStatusDto> GetStatusAsync()
        {
            var pid = await LoadProcessIdAsync();
            if (!pid.HasValue)
            {
                return new WorkerStatusDto { Status = "Stopped" };
            }

            try
            {
                var process = GetProcessById(pid.Value);
                if (process != null && process.ProcessName.ToLower() == "ffmpeg" && !process.HasExited)
                {
                    return new WorkerStatusDto { Status = "running" };
                }
            }
            catch (Exception)
            {
                ClearProcessId();
            }

            return new WorkerStatusDto { Status = "Stopped" };
        }

    }
}
