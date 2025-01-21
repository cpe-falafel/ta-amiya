using System.Text;
using WorkerApi.Exceptions;
using WorkerApi.Models.Graph;
using WorkerApi.Services;

namespace WorkerApi.Models.Filters
{
    public class OutFilter : FilterVertex, IOutFilter
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

        public void AddOutput(int _, VideoCommand cmd)
        {
            if (_videoInName != null)
            {
                cmd.Args.Add("-map");
                cmd.Args.Add($"[{_videoInName}]");
            }

            if (_audioInName != null)
            {
                cmd.Args.Add("-map");
                cmd.Args.Add($"[{_audioInName}]");
            }

            cmd.Args.Add("-f");
            cmd.Args.Add("flv");
            cmd.Args.Add(_dst ?? "");
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
