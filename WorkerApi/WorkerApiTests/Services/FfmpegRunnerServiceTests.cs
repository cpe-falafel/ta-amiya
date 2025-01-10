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

namespace WorkerApi.Services.Tests
{
    [TestClass()]
    public class FfmpegRunnerServiceTests
    {
        private Mock<ILogger<FfmpegRunnerService>> _loggerMock;
        private FfmpegRunnerService _ffmpegRunnerService;

        [TestInitialize]
        public void Initialize()
        {
            _loggerMock = new Mock<ILogger<FfmpegRunnerService>>();
            _ffmpegRunnerService = new FfmpegRunnerService(_loggerMock.Object);
        }

        [TestMethod()]
        public void FfmpegRunnerServiceTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void RunFfmpegCommandAsync_ShouldStartProcess()
        {
            // Arrange
            var mockCommand = new Mock<VideoCommand>();
            mockCommand.Setup(c => c.Compile()).Returns("");
        }

        [TestMethod()]
        public void StopFfmpegCommandTest()
        {
            Assert.Fail();
        }

        [TestMethod()]
        public void StopAllFfmpegProcessesTest()
        {
            Assert.Fail();
        }
    }
}