﻿using WorkerApi.Models;
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
            StopFfmpegCommand(); // Stop any running FFmpeg process

            //var commandTest = "-i rtmp://liveserver:1935/live1/test -vf drawbox=x=100:y=100:w=200:h=200:color=red -c:v libx264 -preset veryfast -pix_fmt yuv420p -c:a copy -f flv rtmp://liveserver:1935/live2/test";

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

        public void StopAllFfmpegProcesses()
        {

            var ffmpegProcesses = _processFactory.GetProcessesByName("ffmpeg");

            foreach (var process in ffmpegProcesses)
            {
                try
                {
                    process.Kill();
                    process.Dispose();

                    _logger.LogInformation($"FFmpeg process {process.ProcessId} stopped");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, $"Error stopping FFmpeg process {process.ProcessId} : {ex.Message}");
                }
            }
        }
    }
}
