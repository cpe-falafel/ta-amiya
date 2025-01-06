using QuickGraph;

namespace WorkerApi.Models.Graph
{
    public class StreamEdge : Edge<FilterVertex>
    {

        public String StreamName { get; }

        public uint SrcOrder { get; }

        public uint DstOrder { get; }


        public StreamEdge(FilterVertex source, FilterVertex target, string name) : base(source, target)
        {
            StreamName = name;
        }
    }
}
