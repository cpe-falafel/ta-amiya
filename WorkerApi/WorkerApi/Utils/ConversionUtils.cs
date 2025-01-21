using System.Drawing;
using System.Globalization;
using System.Text.RegularExpressions;

namespace WorkerApi.Utils
{
    public class ConversionUtils
    {
        public static double? EnsureDouble(object? o) {
            var s = Convert.ToString(o, CultureInfo.InvariantCulture);
            try
            {
                return s == null ? null : double.Parse(s, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        public static int? EnsureInt(object? o)
        {
            var s = Convert.ToString(o, CultureInfo.InvariantCulture);
            try
            {
                return s == null ? null : int.Parse(s, CultureInfo.InvariantCulture);
            }
            catch (FormatException)
            {
                return null;
            }
        }

        public static bool? EnsureBool(object? o)
        {
            var s = Convert.ToString(o, CultureInfo.InvariantCulture)?.ToLower();
            return s?.Equals("true");
        }

        public static string? EnsureColor(object? o)
        {
            string? color = Convert.ToString(o, CultureInfo.InvariantCulture).ToUpper();
            return color== null ? null: (Regex.IsMatch(color, @"^#([A-F0-9]+)$") ? color : null);
        }

        public static string ToStringInvariant(object? o)
        {
            return Convert.ToString(o, CultureInfo.InvariantCulture) ?? "";
        }
    }
}
