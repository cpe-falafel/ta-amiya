using WorkerApi.Models.Graph;
using WorkerApi.Services;

namespace WorkerApi.Models.Filters
{
    public class DrawboxFilter : FilterVertex
    {

        public override string FilterName => "drawbox";

        public override string[] OutStreams { get; }

        public override string[] InStreams { get; }

        public override object[] GetFilterParams()
        {
            return [
                KeyValuePair.Create("x", 0),
                KeyValuePair.Create("y", 0),
                KeyValuePair.Create("x", 0),
                KeyValuePair.Create("y", 0),
            ];
        }

        public DrawboxFilter(string key, FilterGraphItem item): base(key) {
            if (!item.Type.Equals(FilterName)) throw new IOException("Filter name is not matching");
            this.InStreams = item.In.ToArray();
            this.OutStreams = item.Out.ToArray();
        }
    }
}
