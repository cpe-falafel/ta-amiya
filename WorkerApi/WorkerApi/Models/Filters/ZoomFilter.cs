using System.Globalization;
using WorkerApi.Services;
using WorkerApi.Utils;

namespace WorkerApi.Models.Filters
{
    /**
     * Uses split+ crop + scale to zoom
     * (we use split with ref to avoid approximation in output format)
     */
    public class ZoomFilter : AbstractFilterComplexVertex
    {

        public override string FilterName => "zoom";

        public override string[] OutStreams { get; }

        public override string[] InStreams { get; }

        private readonly double _zw;
        private readonly double? _zh;
        private readonly int? _zx;
        private readonly int? _zy;

        public override object[] GetFilterParams()
        {
            return [
                KeyValuePair.Create("zw", _zw),
                KeyValuePair.Create("zh", _zh),
                KeyValuePair.Create("zx", _zx),
                KeyValuePair.Create("zy", _zy),
            ];
        }

        public override string ComputeFilterComplexOutput()
        {
            string zw = ConversionUtils.ToStringInvariant(_zw);
            string zh = ConversionUtils.ToStringInvariant(_zh);
            string zx = ConversionUtils.ToStringInvariant(_zx);
            string zy = ConversionUtils.ToStringInvariant(_zy);
            return $"[{InStreams[0]}]split[{InStreams[0]}:1]," +
                $"crop=w=iw/{zw}:h=ih/{zh}:x=(iw-ow)/2+{zx}:y=(ih-oh)/2-{zy}[{InStreams[0]}:0];" +
                $"[{InStreams[0]}:0][{InStreams[0]}:1]scale=rw:rh[{OutStreams[0]}]";
        }

        public ZoomFilter(string key, FilterGraphItem item): base(key)
        {
            if (!item.Type.Equals(FilterName)) throw new IOException("Filter name is not matching");
            if (item.In.Count != 1) throw new IOException("Split filter takes exactly 1 input");
            if (item.Out.Count != 1) throw new IOException("Splitfilter takes exactly 1 output");
            this.InStreams = item.In.ToArray();
            this.OutStreams = item.Out.ToArray();
            
            object? zw,zh,zx,zy = null;
            item.Properties.TryGetValue("zw", out zw);
            item.Properties.TryGetValue("zh", out zh);
            item.Properties.TryGetValue("zx", out zx);
            item.Properties.TryGetValue("zy", out zy);
            _zw = ConversionUtils.EnsureDouble(zw) ?? 1.0d;
            _zh = ConversionUtils.EnsureDouble(zh) ?? _zw;
            _zx = ConversionUtils.EnsureInt(zx) ?? 0;
            _zy = ConversionUtils.EnsureInt(zy) ?? 0;

        }
    }
}
