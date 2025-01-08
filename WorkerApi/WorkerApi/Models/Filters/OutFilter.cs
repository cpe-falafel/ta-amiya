using System.Text;
using WorkerApi.Exceptions;
using WorkerApi.Models.Graph;
using WorkerApi.Services;

namespace WorkerApi.Models.Filters
{
    public class OutFilter : FilterVertex
    {

        public override string FilterName => "_OUT";

        public override string[] OutStreams { get; } = [];

        public override string[] InStreams { get; }

        private readonly string? _videoInName;
        private readonly string? _audioInName;

        private readonly string? _dst;

        public override object[] GetFilterParams()
        {
            return [
                KeyValuePair.Create("dst",_dst)
            ];
        }

        private void AddCmdArgs(StringBuilder sb, string? outVideoStreamEnvName, string? outAudioStreamEnvName, string outEnvName)
        {
            if (outVideoStreamEnvName != null)
            {
                sb.Append(" -map");
                sb.Append($" \"[${outVideoStreamEnvName}]\"");
            }

            if (outAudioStreamEnvName != null)
            {
                sb.Append(" -map");
                sb.Append($" \"[${outAudioStreamEnvName}]\"");
            }

            sb.Append(" -f");
            sb.Append(" flv");
            sb.Append($" \"${outEnvName}\"");
        }

        public void AddOutput(int idx, VideoCommand cmd)
        {
            var outVideoStreamEnvName= "AMIYA_OUTVID_" + idx.ToString();
            var outAudioStreamEnvName = "AMIYA_OUTAUD_" + idx.ToString();
            var outEnvName = "AMIYA_OUT_" + idx.ToString();

            AddCmdArgs(cmd.Args,
                _videoInName == null ? null : outVideoStreamEnvName,
                _audioInName == null ? null : outAudioStreamEnvName,
                outEnvName);

            cmd.Env.Add(outVideoStreamEnvName, _videoInName ?? "");
            cmd.Env.Add(outAudioStreamEnvName, _audioInName ?? "");
            cmd.Env.Add(outEnvName, _dst ?? "");
        }

        public OutFilter(string key, FilterGraphItem item): base(key)
        {
            if (!item.Type.Equals(FilterName)) throw new FilterException("Filter name is not matching");
            if (item.In.Count != 2) throw new FilterException("Need 2 inputs in OutFilter: [{video}, {audio}]");
            InStreams = item.In.ToArray();
            _dst = item.Properties.GetValueOrDefault("dst", "").ToString();
            _videoInName = InStreams[0];
            _audioInName = InStreams[1];
        }
    }
}
