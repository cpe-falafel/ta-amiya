using Microsoft.VisualBasic;
using System.Text;
using WorkerApi.Exceptions;
using WorkerApi.Models.Graph;
using WorkerApi.Services;
using WorkerApi.Utils;

namespace WorkerApi.Models.Filters
{
    public class InFilter : AbstractFilterComplexVertex, IInFilter
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
            cmd.Args.Add("-i");
            cmd.Args.Add(_src);

            _lastIdx = idx;
        }

        public override string ComputeFilterComplexOutput()
        {
            return BuildFilter(_lastIdx);
        }

        public InFilter(string key, FilterGraphItem item): base(key) 
        {
            if (!item.Type.Equals(FilterName)) throw new FilterException("Filter name is not matching");
            if (item.Out.Count != 2) throw new FilterException("Need 2 out in InFilter: [{video}, {audio}]");

            var src = item.Properties.GetValueOrDefault("src", "").ToString();
            if (!ProtocolUtils.IsRtmpProtocol(src)) throw new FilterException("Input source must be an rtmp stream");

            OutStreams = item.Out.ToArray();
            _videoOutName = OutStreams[0];
            _audioOutName = OutStreams[1];
            _src = src;
        }
    }
}
