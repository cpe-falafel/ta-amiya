using Microsoft.Extensions.FileSystemGlobbing.Internal;
using Microsoft.Extensions.FileSystemGlobbing;
using Newtonsoft.Json;
using QuickGraph;
using WorkerApi.Models.Graph;
using System.Text.RegularExpressions;

namespace WorkerApi.Services
{
    public class FilterGraphService : IFilterGraphService
    {
        private readonly ILogger<FilterGraphService> _logger;

        public FilterGraphService(ILogger<FilterGraphService> logger)
        {
            _logger = logger;
        }

        public BidirectionalGraph<FilterVertex, StreamEdge> ConvertToGraph(string json) {
            Dictionary<string, FilterGraphItem>? filterDict = JsonConvert.DeserializeObject<Dictionary<string, FilterGraphItem>>(json);

            if (filterDict == null)
            {
                throw new IOException("JSON could not be read as Dict");
            }
            var graph = new BidirectionalGraph<FilterVertex, StreamEdge>();

            Dictionary<string, StreamGraphItem> streamDict = AddFilters(graph, filterDict);
            AddStreams(graph, streamDict);
            return graph;

        }

        private void AddStreams(BidirectionalGraph<FilterVertex, StreamEdge> graph, Dictionary<string, StreamGraphItem> streamDict)
        {
            foreach (StreamGraphItem item in streamDict.Select(kv => kv.Value))
            {
                FilterVertex? inVertex = graph.Vertices.FirstOrDefault(v => v.Key.Equals(item.InKey), null);
                FilterVertex? outVertex = graph.Vertices.FirstOrDefault(v => v.Key.Equals(item.OutKey), null);
                if (inVertex == null || outVertex == null)
                {
                    _logger.LogWarning("Could not find all vertices for keys {} and {}", item.InKey, item.OutKey);
                    continue;
                }
                graph.AddEdge(new StreamEdge(inVertex, outVertex, item.Name));
            }
        }

        private Dictionary<string, StreamGraphItem> AddFilters(BidirectionalGraph<FilterVertex, StreamEdge> graph,  IReadOnlyDictionary<string, FilterGraphItem> filterDict)
        {
            int count = 0;
            Dictionary<string, StreamGraphItem> streamDict = new Dictionary<string, StreamGraphItem>();
            foreach (KeyValuePair<string, FilterGraphItem> itemKv in filterDict)
            {
                var outRealNames = new List<string?>();
                foreach (string streamName in itemKv.Value.Out)
                {
                    if (streamName == null)
                    {
                        outRealNames.Add(null);
                        continue;
                    }
                    if (streamDict.TryAdd(streamName, new StreamGraphItem("s" + count.ToString()))) { count++; }
                    StreamGraphItem? item = null;
                    if (streamDict.TryGetValue(streamName, out item)) { 
                        item.InKey = itemKv.Key;
                        outRealNames.Add(item.Name);
                    };
                }

                var inRealNames = new List<string?>();
                foreach (string streamName in itemKv.Value.In)
                {
                    if (streamName == null)
                    {
                        inRealNames.Add(null);
                        continue;
                    }
                    if (streamDict.TryAdd(streamName, new StreamGraphItem("s" + count.ToString()))) { count++; }
                    StreamGraphItem? item = null;
                    if (streamDict.TryGetValue(streamName, out item))
                    {
                        item.OutKey = itemKv.Key;
                        inRealNames.Add(item.Name);
                    };
                }
                FilterVertex? filterVertex = FilterVertexFactory.GenerateFilterVertex(itemKv.Key, new FilterGraphItem()
                {
                    In = inRealNames,
                    Out = outRealNames,
                    Properties = itemKv.Value.Properties,
                    Type = itemKv.Value.Type
                });
                if (filterVertex != null) { graph.AddVertex(filterVertex); }
            }
            return streamDict;
        }
    }

    public class FilterGraphItem
    {
        public string Type { get; set; }
        public List<string?> In { get; set; }
        public List<string?> Out { get; set; }
        public Dictionary<string, object> Properties { get; set; }
    }

    public class StreamGraphItem
    {

        public string Name { get; }

        public string InKey { get; set; }
        public string OutKey { get; set; }

        public StreamGraphItem(String name)
        {
            if (Regex.IsMatch(name, "[^a-zA-Z0-9]")) throw new IOException("StreamGraphItem name should be alphanum");
            this.Name = name;
        }
    }
}
