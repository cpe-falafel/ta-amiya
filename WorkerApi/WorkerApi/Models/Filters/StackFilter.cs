using WorkerApi.Services;
using WorkerApi.Utils;

namespace WorkerApi.Models.Filters
{
    /**
     * Uses split + scale +hstack of vstack to adapt and stack
     */
    public class StackFilter : AbstractFilterComplexVertex
    {

        public override string FilterName => "stack";

        public override string[] OutStreams { get; }

        public override string[] InStreams { get; }

        private readonly bool _horizontalMode;

        public override object[] GetFilterParams()
        {
            return [
                KeyValuePair.Create("horizontal_mode", _horizontalMode)
            ];
        }

        public override string ComputeFilterComplexOutput()
        {
            string stackFiltername = _horizontalMode ? "hstack" : "vstack";
            // If horizontal: match height, if vertical: match width
            string scaleFilterArg = _horizontalMode ? "-1:rh" : "rw:-1";
            return $"[{InStreams[1]}]split[{InStreams[1]}:0][{InStreams[1]}:1];[{InStreams[0]}][{InStreams[1]}:0]scale={scaleFilterArg},[{InStreams[1]}:1]{stackFiltername}[{OutStreams[0]}]";
        }

        public StackFilter(string key, FilterGraphItem item): base(key)
        {
            if (!item.Type.Equals(FilterName)) throw new IOException("Filter name is not matching");
            if (item.In.Count != 2) throw new IOException("Stack filter takes exactly 2 input");
            if (item.Out.Count != 1) throw new IOException("Stack filter takes exactly 1 output");
            this.InStreams = item.In.ToArray();
            this.OutStreams = item.Out.ToArray();

            _horizontalMode = ConversionUtils.EnsureBool(item.Properties.GetValueOrDefault("horizontal_mode", "false")) ?? false;
        }
    }
}
