using WorkerApi.Models.Graph;
using WorkerApi.Services;

namespace WorkerApi.Models.Filters
{
    public class DrawboxFilter : AbstractFilterComplexVertex
    {

        public override string FilterName => "drawbox";

        public override string[] OutStreams { get; }

        public override string[] InStreams { get; }

        private readonly string? _left;
        private readonly string? _right;
        private readonly string? _top;
        private readonly string? _bottom;
        private readonly string? _color;
        private readonly string? _thickness;

        public override object[] GetFilterParams()
        {
            return [
                KeyValuePair.Create("left", _left ?? "0"),
                KeyValuePair.Create("right", _right ?? "0"),
                KeyValuePair.Create("top", _top ?? "0"),
                KeyValuePair.Create("bottom", _bottom ?? "0"),
                KeyValuePair.Create("thickness", _thickness ?? "1"),
                KeyValuePair.Create("color", _color ?? "red")
            ];
        }

        public override string ComputeFilterComplexOutput()
        {
            var p = GetFilterParams()
                .Cast<KeyValuePair<string, string>>() // Cast des objets en KeyValuePair<string, int>
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value); // Conversion en dictionnaire pour accès par clé

            // Construction de la commande avec les valeurs récupérées
            return $"[{InStreams[0]}]drawbox=x={p["left"]}:y={p["top"]}:w=iw-{p["right"]}-x:h=ih-{p["bottom"]}-y:color={p["color"]}:t={p["thickness"]}[{OutStreams[0]}]";
        }

        public DrawboxFilter(string key, FilterGraphItem item): base(key)
        {
            if (!item.Type.Equals(FilterName)) throw new IOException("Filter name is not matching");
            this.InStreams = item.In.ToArray();
            this.OutStreams = item.Out.ToArray();

            _left = item.Properties.GetValueOrDefault("left", "").ToString();
            _right = item.Properties.GetValueOrDefault("right", "").ToString();
            _top = item.Properties.GetValueOrDefault("top", "").ToString();
            _bottom = item.Properties.GetValueOrDefault("bottom", "").ToString();
            _color = item.Properties.GetValueOrDefault("color", "").ToString();
            _thickness = item.Properties.GetValueOrDefault("thickness", "").ToString();
        }
    }
}
