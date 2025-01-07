using WorkerApi.Models.Graph;
using WorkerApi.Services;

namespace WorkerApi.Models.Filters
{
    public class OutFilter : FilterVertex
    {

        public override string FilterName => "_OUT";

        public override string[] OutStreams { get; } = [];

        public override string[] InStreams { get; }

        private readonly string? dst;

        public override object[] GetFilterParams()
        {
            return [
                KeyValuePair.Create("dst",dst)
            ];
        }

        public OutFilter(string key, FilterGraphItem item): base(key) {
            if (!item.Type.Equals(FilterName)) throw new IOException("Filter name is not matching");
            this.InStreams = item.In.ToArray();
            dst = item.Properties.GetValueOrDefault("dst", "").ToString();
        }
    }
}
