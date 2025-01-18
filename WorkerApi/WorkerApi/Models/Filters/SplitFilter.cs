using WorkerApi.Services;
using WorkerApi.Utils;

namespace WorkerApi.Models.Filters
{
    public class SplitFilter : AbstractFilterComplexVertex
    {

        public override string FilterName => "split";

        public override string[] OutStreams { get; }

        public override string[] InStreams { get; }

        private readonly bool _audioMode;

        public override object[] GetFilterParams()
        {
            return [
                KeyValuePair.Create("audio_mode", _audioMode)
            ];
        }

        public override string ComputeFilterComplexOutput()
        {
            string filtername = _audioMode ? "asplit" : "split";
            return $"[{InStreams[0]}]{filtername}[{OutStreams[0]}][{OutStreams[1]}]";
        }

        public SplitFilter(string key, FilterGraphItem item): base(key)
        {
            if (!item.Type.Equals(FilterName)) throw new IOException("Filter name is not matching");
            if (item.In.Count != 1) throw new IOException("Split filter takes exactly 1 input");
            if (item.Out.Count != 2) throw new IOException("Splitfilter takes exactly 2 output");
            this.InStreams = item.In.ToArray();
            this.OutStreams = item.Out.ToArray();

            _audioMode = ConversionUtils.EnsureBool(item.Properties.GetValueOrDefault("audio_mode", "false")) ?? false;
        }
    }
}
