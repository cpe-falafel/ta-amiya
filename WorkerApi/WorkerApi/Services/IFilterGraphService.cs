using QuickGraph;
using WorkerApi.Models.Graph;

namespace WorkerApi.Services
{
    public interface IFilterGraphService
    {
        public BidirectionalGraph<FilterVertex, StreamEdge> ConvertToGraph(string json);
    }
}
