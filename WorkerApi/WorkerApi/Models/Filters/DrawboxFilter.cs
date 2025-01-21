using Microsoft.VisualBasic;
using WorkerApi.Models.Graph;
using WorkerApi.Services;
using WorkerApi.Utils;

namespace WorkerApi.Models.Filters
{
    public class DrawboxFilter : AbstractFilterComplexVertex
    {

        public override string FilterName => "drawbox";

        public override string[] OutStreams { get; }

        public override string[] InStreams { get; }

        // TODO: secure this by using integers
        private readonly int _left;
        private readonly int _right;
        private readonly int _top;
        private readonly int _bottom;
        private readonly string? _color;
        private readonly int _thickness;

        public override object[] GetFilterParams()
        {
            return [
                KeyValuePair.Create("left", _left),
                KeyValuePair.Create("right", _right),
                KeyValuePair.Create("top", _top),
                KeyValuePair.Create("bottom", _bottom),
                KeyValuePair.Create("thickness", _thickness),
                KeyValuePair.Create("color", _color)
            ];
        }

        public override string ComputeFilterComplexOutput()
        {
            string left = ConversionUtils.ToStringInvariant(_left);
            string right = ConversionUtils.ToStringInvariant(_right);
            string top = ConversionUtils.ToStringInvariant(_top);
            string bottom = ConversionUtils.ToStringInvariant(_bottom);
            string thickness = ConversionUtils.ToStringInvariant(_thickness);


            // Construction de la commande avec les valeurs récupérées
            return $"[{InStreams[0]}]drawbox=x={left}:y={top}:w=iw-{right}-x:h=ih-{bottom}-y:color={_color}:t={thickness}[{OutStreams[0]}]";
        }

        public DrawboxFilter(string key, FilterGraphItem item): base(key)
        {
            if (!item.Type.Equals(FilterName)) throw new IOException("Filter name is not matching");
            this.InStreams = item.In.ToArray();
            this.OutStreams = item.Out.ToArray();

            object? left, right, top, bottom, thickness;

            item.Properties.TryGetValue("left", out left);
            item.Properties.TryGetValue("right", out right);
            item.Properties.TryGetValue("top", out top);
            item.Properties.TryGetValue("bottom", out bottom);
            item.Properties.TryGetValue("thickness", out thickness);

            _left = ConversionUtils.EnsureInt(left) ?? 0;
            _right = ConversionUtils.EnsureInt(right) ?? 0;
            _top = ConversionUtils.EnsureInt(top) ?? 0;
            _bottom = ConversionUtils.EnsureInt(bottom) ?? 0;
            _thickness = ConversionUtils.EnsureInt(thickness) ?? 1;
            _color = ConversionUtils.EnsureColor(item.Properties?.GetValueOrDefault("color", null)) ?? "#000000";
        }
    }
}
