﻿using WorkerApi.Models;
using System.Diagnostics;
using WorkerApi.Services.Process;
using Serilog;
using Serilog.Core;
using System.Text.Json;
using WorkerApi.Models.DTO;

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
            var pid = await LoadProcessIdAsync();
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
                    process.WaitForExitAsync();
                    _logger.LogInformation($"FFmpeg process {process.Id} stopped");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error stopping FFmpeg process {process.Id} : {ex.Message}");
                }
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
                var process = System.Diagnostics.Process.GetProcessById(pid.Value);
                if (process.ProcessName.ToLower() == "ffmpeg" && !process.HasExited)
                {
                    return new WorkerStatusDto { Status = "running" };
                }
            }
            catch (Exception ex)
            {
                await ClearProcessIdAsync();
            }

            return new WorkerStatusDto { Status = "Stopped" };
        }
    }
}
