using WorkerApi.Models.Graph;
using WorkerApi.Services;

namespace WorkerApi.Models.Filters
{
    public class DrawboxFilter : AbstractFilterComplexVertex
    {

        public override string FilterName => "drawbox";

        public override string[] OutStreams { get; }

        public override string[] InStreams { get; }

        public override object[] GetFilterParams()
        {
            return [
                KeyValuePair.Create("left", 0),
                KeyValuePair.Create("right", 0),
                KeyValuePair.Create("top", 0),
                KeyValuePair.Create("bottom", 0),
            ];
        }

        public override string ComputeFilterComplexOutput()
        {
            var p = GetFilterParams()
                .Cast<KeyValuePair<string, int>>() // Cast des objets en KeyValuePair<string, int>
                .ToDictionary(kvp => kvp.Key, kvp => kvp.Value); // Conversion en dictionnaire pour accès par clé

            // Construction de la commande avec les valeurs récupérées
            return $"[{InStreams[0]}]drawbox=x={p["left"]}:y={p["top"]}:w=iw-{p["right"]}-x:h=ih-{p["bottom"]}-y[{OutStreams[0]}]";
        }

        public DrawboxFilter(string key, FilterGraphItem item): base(key)
        {
            if (!item.Type.Equals(FilterName)) throw new IOException("Filter name is not matching");
            this.InStreams = item.In.ToArray();
            this.OutStreams = item.Out.ToArray();
        }
    }
}
