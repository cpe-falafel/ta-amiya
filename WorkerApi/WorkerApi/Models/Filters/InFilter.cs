using WorkerApi.Models.Graph;
using WorkerApi.Services;

namespace WorkerApi.Models.Filters
{
    public class InFilter : FilterVertex
    {

        public override string FilterName => "_IN";

        public override string[] OutStreams { get; }

        public override string[] InStreams { get; } = [];

        private string? src;

        public override object[] GetFilterParams()
        {
            return [
                KeyValuePair.Create("src",src)
            ];
        }

        public InFilter(string key, FilterGraphItem item): base(key) {
            if (!item.Type.Equals(FilterName)) throw new IOException("Filter name is not matching");
            this.OutStreams = item.Out.ToArray();
            src = item.Properties.GetValueOrDefault("src", "").ToString();
        }
    }
}
