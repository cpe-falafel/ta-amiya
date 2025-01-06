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
            var loggerMock = new Mock<ILogger<IFilterGraphService>>();
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
        }
    }
}