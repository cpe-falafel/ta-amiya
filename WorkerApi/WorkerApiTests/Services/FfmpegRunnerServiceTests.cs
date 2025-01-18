using Microsoft.VisualStudio.TestTools.UnitTesting;
using WorkerApi.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Moq;
using Castle.Core.Logging;
using Microsoft.Extensions.Logging;
using WorkerApi.Models;
using QuickGraph.Algorithms.Services;
using WorkerApi.Services.Process;
using System.Diagnostics;

namespace WorkerApi.Services.Tests
{
    [TestClass()]
    public class FfmpegRunnerServiceTests
    {
        private Mock<ILogger<FfmpegRunnerService>> _loggerMock;
        private Mock<IProcessFactory> _processFactoryMock;
        private Mock<IProcessWrapper> _processMock;
        private FfmpegRunnerService _service;

        [TestInitialize]
        public void Setup() 
        {
            _loggerMock = new Mock<ILogger<FfmpegRunnerService>>();
            _processFactoryMock = new Mock<IProcessFactory>();
            _processMock = new Mock<IProcessWrapper>();
            _service = new FfmpegRunnerService(_loggerMock.Object, _processFactoryMock.Object);
        }

        [TestMethod]
        public async Task RunFfmpegCommandAsync_ShouldStartAndConfigureProcess()
        {
            // Arrange
            var videoCommand = new VideoCommand();
            videoCommand.Args.Append("-i input.mp4 -c:v libx264 output.mp4");

            _processFactoryMock.Setup(f => f.CreateProcess())
                .Returns(_processMock.Object);

            // Act
            await _service.RunFfmpegCommandAsync(videoCommand);

            // Assert
            _processMock.VerifySet(p => p.StartInfo = It.Is<ProcessStartInfo>(si =>
                si.FileName == "ffmpeg" &&
                si.Arguments == videoCommand.Compile() &&
                si.RedirectStandardOutput == true &&
                si.RedirectStandardError == true &&
                si.UseShellExecute == false));

            _processMock.VerifySet(p => p.EnableRaisingEvents = true);
            _processMock.Verify(p => p.Start(), Times.Once);
            _processMock.Verify(p => p.BeginOutputReadLine(), Times.Once);
            _processMock.Verify(p => p.BeginErrorReadLine(), Times.Once);
            _processMock.Verify(p => p.WaitForExitAsync(), Times.Once);
        }

        [TestMethod]
        public void StopFfmpegCommand_ShouldKillRunningProcess()
        {
            // Arrange
            _processMock.Setup(p => p.HasExited).Returns(false);
            _processFactoryMock.Setup(f => f.CreateProcess())
                .Returns(_processMock.Object);

            // Initialize _ffmpegProcess in the service
            var videoCommand = new VideoCommand();
            videoCommand.Args.Append("-i input.mp4 output.mp4");
            _service.RunFfmpegCommandAsync(videoCommand).Wait(); // Simule l'initialisation

            // Act
            _service.StopFfmpegCommand();

            // Assert
            _processMock.Verify(p => p.Kill(), Times.Once); // Verify Kill was called
            _processMock.Verify(p => p.Dispose(), Times.Once); // Verify Dispose was called
        }

        [TestMethod]
        public void StopAllFfmpegProcesses_ShouldKillAllProcesses()
        {
            // Arrange
            var process1Mock = new Mock<IProcessWrapper>();
            var process2Mock = new Mock<IProcessWrapper>();
            var processes = new List<IProcessWrapper> { process1Mock.Object, process2Mock.Object };

            _processFactoryMock.Setup(f => f.GetProcessesByName("ffmpeg"))
                .Returns(processes);

            // Act
            _service.StopAllFfmpegProcesses();

            // Assert
            process1Mock.Verify(p => p.Kill(), Times.Once);
            process1Mock.Verify(p => p.Dispose(), Times.Once);
            process2Mock.Verify(p => p.Kill(), Times.Once);
            process2Mock.Verify(p => p.Dispose(), Times.Once);
        }
    }
}