using Microsoft.Extensions.Logging;
using Moq;
using QuickGraph.Algorithms.Services;

namespace WorkerApi.Services.Tests
{
    [TestClass]
    public class FilterGraphServiceTests
    {

        private String ReadJson(String filename) {
            string folderPath = Path.Combine(Path.Combine(Directory.GetCurrentDirectory(), "Resources"),"FilterGraphService");
            return File.ReadAllText(Path.Combine(folderPath, filename));
        }

        [TestMethod]
        public void ConvertToGraphTest()
        {
            var loggerMock = new Mock<ILogger<FilterGraphService>>();
            var filterGraphService= new FilterGraphService(loggerMock.Object);

            var graph = filterGraphService.ConvertToGraph(ReadJson("graph1.json"));
            Assert.IsNotNull(graph);
            Assert.AreEqual(graph.EdgeCount, 2);
            Assert.AreEqual(graph.VertexCount, 3);
            Assert.AreEqual(
                graph.Edges.First(e => e.Source.FilterName.Equals("_IN")).Target.FilterName,
                "drawbox"
            );
            Assert.AreEqual(
                graph.Edges.First(e => e.Source.FilterName.Equals("drawbox")).Target.FilterName,
                "_OUT"
            );

            Assert.AreEqual(
                graph.Vertices.First(v => v.FilterName.Equals("_IN")).GetFilterParams()[0],
                KeyValuePair.Create("src", "rtmp://localhost/mystream1")
            );

            Assert.AreEqual(
                graph.Vertices.First(v => v.FilterName.Equals("_OUT")).GetFilterParams()[0],
                KeyValuePair.Create("dst", "rtmp://localhost/mystream2")
            );

            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not find all vertices")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                ),
                Times.Exactly(0)
            );
        }

        [TestMethod]
        public void ConvertToInvalidGraphTest()
        {
            var loggerMock = new Mock<ILogger<FilterGraphService>>();
            var filterGraphService = new FilterGraphService(loggerMock.Object);

            var graph = filterGraphService.ConvertToGraph(ReadJson("graph2.json"));
            Assert.IsNotNull(graph);
            Assert.AreEqual(graph.EdgeCount, 0);
            Assert.AreEqual(graph.VertexCount, 2);
            
            loggerMock.Verify(
                x => x.Log(
                    It.Is<LogLevel>(l => l == LogLevel.Warning),
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((v, t) => v.ToString().Contains("Could not find all vertices")),
                    It.IsAny<Exception>(),
                    It.Is<Func<It.IsAnyType, Exception, string>>((v, t) => true)
                ),
                Times.Exactly(2)
            );
        }
    }
}