using WorkerApi.Exceptions;
using WorkerApi.Services;
using WorkerApi.Utils;

namespace WorkerApi.Models.Filters
{
    public class WrappedWithCheckOutFilter : AbstractFilterComplexVertex, IOutFilter
    {
        private class CheckedOutOption
        {
            public CheckedOutOption(string outVideo, string outJpg) {
                ToOutName = outVideo + ":3";
                ToJpgName = outVideo + ":4";
                JpgPath = outJpg;
            }
            public string ToOutName { get; }
            public string ToJpgName { get; }
            public string JpgPath { get; }
        }

        private readonly CheckedOutOption? _options;


        private readonly OutFilter _out;

        
        public override string FilterName => "_CHECKED_OUT";

        public override string[] OutStreams { get { return _out.OutStreams; } }

        public override string[] InStreams { get; }
        private readonly uint? _minScore;

        public override object[] GetFilterParams()
        {
            return _out.GetFilterParams().Concat([
                KeyValuePair.Create("min_score", _minScore)
            ]).ToArray();
        }

        public void AddOutput(int i, VideoCommand cmd)
        {
            if (_minScore.HasValue) cmd.MinScore = _minScore.Value;
            if (_options != null)
            {
                cmd.Args.Add("-map");
                cmd.Args.Add($"[{_options.ToJpgName}]");
                cmd.Args.Add($"-update");
                cmd.Args.Add($"1");
                cmd.Args.Add($"-y");
                cmd.Args.Add(_options.JpgPath);
            }
            _out.AddOutput(i, cmd);
        }

        public override string ComputeFilterComplexOutput()
        {
            if (_options == null) return "";
            var outVideo = this.InStreams[0];
            var outJpgFilter = "fps=1/10,crop=min(iw\\,ih):min(iw\\,ih):(iw-min(iw\\,ih))/2:(ih-min(iw\\,ih))/2,scale=256:256";

            var blurFilter = $"zmq,gblur@censor=sigma=50:enable=0";
            return $"[{outVideo}]split[{outVideo}:0],"+
                $"{outJpgFilter}[{_options.ToJpgName}];"+
                $"[{outVideo}:0]{blurFilter}[{_options.ToOutName}]";
        }

        public WrappedWithCheckOutFilter(string key, FilterGraphItem item): base(key)
        {
            if (!item.Type.Equals(FilterName)) throw new FilterException("Filter name is not matching");
            if (item.In.Count != 2) throw new FilterException("Need 2 inputs in OutFilter: [{video}, {audio}]");

            var outVideo = item.In[0];
            InStreams = item.In.ToArray();
            string? outJpg = Environment.GetEnvironmentVariable("AMIYA_OUTJPG");

            if (outVideo != null && outJpg != null)
            {
                _options = new CheckedOutOption(outVideo, outJpg);
            }

            item.Properties.TryGetValue("min_score", out object? minScoreObj);
            var minScore = ConversionUtils.EnsureInt(minScoreObj ?? 50);

            _minScore = minScore > 0 ? (uint)minScore : null;
            _out = new OutFilter(key, new FilterGraphItem { 
                In = new List<string?>([_options?.ToOutName, item.In[1]]),
                Out = item.Out, 
                Type = "_OUT", 
                Properties = item.Properties 
            });
        }
    }
}
