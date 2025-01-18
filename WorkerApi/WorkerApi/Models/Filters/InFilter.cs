using System.Text;
using WorkerApi.Exceptions;
using WorkerApi.Models.Graph;
using WorkerApi.Services;

namespace WorkerApi.Models.Filters
{
    public class InFilter : AbstractFilterComplexVertex
    {

        public override string FilterName => "_IN";

        public override string[] OutStreams { get; }

        public override string[] InStreams { get; } = [];

        private readonly string? _src;

        private int _lastIdx;

        private readonly string? _videoOutName;
        private readonly string? _audioOutName;

        public override object[] GetFilterParams()
        {
            return [
                KeyValuePair.Create("src",_src)
            ];
        }

        private void AddCmdArgs(StringBuilder sb, string inEnvName)
        {
            sb.Append(" -i");
            sb.Append($" \"${inEnvName}\"");
        }

        private string BuildFilter(int idx)
        {
            var filters = new List<String>();

            if (_videoOutName != null && _audioOutName != null)
            {
                filters.Add($"[{idx}:v]null[{_videoOutName}]");
                filters.Add($"[{idx}:a]anull[{_audioOutName}]");
            } else if (_videoOutName != null)
            {
                filters.Add($"[{idx}]null[{_videoOutName}]");
            } else if (_audioOutName != null)
            {
                filters.Add($"[{idx}]anull[{_audioOutName}]");
            }
            return String.Join(";", filters);
        }


        public void AddInput(int idx, VideoCommand cmd)
        {
            var inEnvName = "AMIYA_IN_" + idx.ToString();

            AddCmdArgs(cmd.Args, inEnvName);

            cmd.Env.Add(inEnvName, _src ?? "");

            _lastIdx = idx;
        }

        public override string ComputeFilterComplexOutput()
        {
            return BuildFilter(_lastIdx);
        }

        public InFilter(string key, FilterGraphItem item): base(key) {
            if (!item.Type.Equals(FilterName)) throw new FilterException("Filter name is not matching");
            if (item.Out.Count != 2) throw new FilterException("Need 2 out in InFilter: [{video}, {audio}]");
            OutStreams = item.Out.ToArray();
            _videoOutName = OutStreams[0];
            _audioOutName = OutStreams[1];

            _src = item.Properties.GetValueOrDefault("src", "").ToString();
        }
    }
}
