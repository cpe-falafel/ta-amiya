using Newtonsoft.Json;
using QuickGraph;
using WorkerApi.Models.Filters;
using WorkerApi.Models.Graph;

namespace WorkerApi.Services
{
    class FilterVertexFactory
    {
        public static FilterVertex GenerateFilterVertex(string key, FilterGraphItem item) {
            switch (item.Type)
            {
                case "drawbox":
                    return new DrawboxFilter(key, item);
                default:
                    return null!;
            }
        }
    }

}
