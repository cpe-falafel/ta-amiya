using Newtonsoft.Json;
using QuickGraph;
using WorkerApi.Models.Filters;
using WorkerApi.Models.Graph;

namespace WorkerApi.Services
{
    class FilterVertexFactory
    {
        public static FilterVertex? GenerateFilterVertex(string key, FilterGraphItem item) {
            switch (item.Type)
            {
                case "drawbox":
                    return new DrawboxFilter(key, item);
                case "split":
                    return new SplitFilter(key, item);
                case "stack":
                    return new StackFilter(key, item);
                case "flip":
                    return new FlipFilter(key, item);
                case "_IN":
                    return new InFilter(key, item);
                case "_OUT":
                    return new OutFilter(key, item);
                default:
                    return null;
            }
        }
    }

}
