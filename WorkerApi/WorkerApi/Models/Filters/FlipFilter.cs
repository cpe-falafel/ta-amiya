using WorkerApi.Models.Graph;
using WorkerApi.Services;

namespace WorkerApi.Models.Filters
{
    public class FlipFilter : AbstractFilterComplexVertex
    {

        public override string FilterName => "flip";

        public override string[] OutStreams { get; }

        public override string[] InStreams { get; }

        private readonly bool _h;
        private readonly bool _v;

        public override object[] GetFilterParams()
        {
            return [
                KeyValuePair.Create("h", _h),
                KeyValuePair.Create("v", _v)
            ];
        }

        public override string ComputeFilterComplexOutput()
        {
            if (!_h && !_v)
            {
                return $"[{InStreams[0]}]null[{OutStreams[0]}]";
            }
            List<string> filters = new List<string>();
            if (_h) filters.Add("hflip");
            if (_v) filters.Add("vflip");
            return $"[{InStreams[0]}]{String.Join(',', filters)}[{OutStreams[0]}]";
        }

        public FlipFilter(string key, FilterGraphItem item): base(key)
        {
            if (!item.Type.Equals(FilterName)) throw new IOException("Filter name is not matching");
            if (item.In.Count != 1) throw new IOException("Flip filter takes exactly 1 input");
            if (item.Out.Count != 1) throw new IOException("Flip filter takes exactly 1 output");
            this.InStreams = item.In.ToArray();
            this.OutStreams = item.Out.ToArray();

            _h = item.Properties.GetValueOrDefault("h", "true")?.ToString()?.ToLower()?.Equals("true") ?? true;
            _v = item.Properties.GetValueOrDefault("v", "false")?.ToString()?.ToLower()?.Equals("true") ?? false;
        }
    }
}
