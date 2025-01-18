using System.Text;
using System.Text.RegularExpressions;

namespace WorkerApi.Models
{
    public class VideoCommand
    {
        public StringBuilder Args { get; } = new StringBuilder();

        public Dictionary<string, string> Env { get; } = new Dictionary<string, string>();

        public string Compile()
        {
            string argsString = Args.ToString();
            string pattern = @"\$([A-Z_0-9]+)";

            argsString = Regex.Replace(argsString, pattern, match =>
            {
                string key = match.Groups[1].Value;
                if (Env.TryGetValue(key, out string? value))
                {
                    return (value??"").Replace("\\", "\\\\").Replace("\"","\\\"");
                }
                return match.Value; // Return the original match if the key is not found in Env
            });

            return argsString;
        }
    }
}
