using WorkerApi.Models.Graph;
using WorkerApi.Services;
using System.Text.RegularExpressions;
using System.Globalization;
using WorkerApi.Utils;
using static System.Net.Mime.MediaTypeNames;

namespace WorkerApi.Models.Filters
{
    public class DrawtextFilter : AbstractFilterComplexVertex
    {


        public enum TextAlign { Left, Right, Center };

        public override string FilterName => "drawtext";

        public override string[] OutStreams { get; }

        public override string[] InStreams { get; }

        private readonly double _y;
        private readonly TextAlign _align;
        private readonly string _color;
        private readonly int _fontSize;
        private readonly string _text;

        public override object[] GetFilterParams()
        {
            return [
                KeyValuePair.Create("y", _y),
                KeyValuePair.Create("font_size", _fontSize),
                KeyValuePair.Create("color", _color),
                KeyValuePair.Create("align", _align),
                KeyValuePair.Create("text", _text)
            ];
        }

        private string SanitizeFfmpegSpecials(string arg)
        {
            return arg.Replace("'", "'\\\\\\''")
                .Replace(",", "\\,")
                .Replace("%", "\\%")
                .Replace(":", "\\:")
                .Replace(";", "\\;");
        }

        public override string ComputeFilterComplexOutput()
        {
            string x = "", align = "";
            switch (_align)
            {
                case TextAlign.Left:
                    x = "0";
                    align = "ML";
                    break;
                case TextAlign.Right:
                    x = "w";
                    align = "MR";
                    break;
                case TextAlign.Center:
                    x = "w/2";
                    align = "MC";
                    break;
            }

            string y = ConversionUtils.ToStringInvariant(_y);
            string fontSize = ConversionUtils.ToStringInvariant(_fontSize);

            string text = _text;
            text = Regex.Replace(text, @"[\s\u00A0]", " ");
            // Replacing antislash & all non ascii-extended to "_"
            text = Regex.Replace(text.Replace("\\", "_"), @"[^\x00-\xFF]", "_");
            text = SanitizeFfmpegSpecials(text);

            // Construction de la commande avec les valeurs récupérées
            return $"[{InStreams[0]}]drawtext=x={x}:y={y}*h:fontcolor={_color}:text='{text}':fontsize={fontSize}[{OutStreams[0]}]";
        }

        public DrawtextFilter(string key, FilterGraphItem item): base(key)
        {
            if (!item.Type.Equals(FilterName)) throw new IOException("Filter name is not matching");
            this.InStreams = item.In.ToArray();
            this.OutStreams = item.Out.ToArray();

            object? y, fontSize;

            item.Properties.TryGetValue("y", out y);
            item.Properties.TryGetValue("font_size", out fontSize);

            _y = ConversionUtils.EnsureDouble(y) ?? 0.5d;
            _fontSize = ConversionUtils.EnsureInt(fontSize) ?? 16;
            _color = ConversionUtils.EnsureColor(item.Properties?.GetValueOrDefault("color", null)) ?? "#000000"; 

            string align = item.Properties?.GetValueOrDefault("align", "center")?.ToString()?.ToLower() ?? "center";
            _align = align.Equals("center") ? TextAlign.Center : (align.Equals("right") ? TextAlign.Right : TextAlign.Left);

            _text = item.Properties?.GetValueOrDefault("text", "")?.ToString() ?? "";
        }
    }
}
