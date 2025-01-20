using System.Text;
using WorkerApi.Exceptions;
using WorkerApi.Models.Graph;
using WorkerApi.Services;
using WorkerApi.Utils;

namespace WorkerApi.Models.Filters
{
    public class WrappedWithCheckOutFilter : AbstractFilterComplexVertex, IInFilter, IOutFilter
    {

        private readonly OutFilter _out;

        private readonly string? _fromOutName;
        private readonly string? _toOutName;
        private readonly string? _toJpgName;



        public override string FilterName => "_CHECKED_OUT";

        public override string[] OutStreams { get { return _out.OutStreams; } }

        public override string[] InStreams { get { return _out.InStreams; } }

        private readonly uint _minScore;

        private readonly string _outputFile;

        public override object[] GetFilterParams()
        {
            return _out.GetFilterParams().Concat([
                KeyValuePair.Create("min_score", _minScore)
            ]).ToArray();
        }

        public void AddOutput(int i, VideoCommand cmd)
        {
            if (_toJpgName != null)
            {
                cmd.Args.Add("-map");
                cmd.Args.Add($"[{_toJpgName}]");
                cmd.Args.Add($"-update");
                cmd.Args.Add($"1");
                cmd.Args.Add($"1");
            }
            _out.AddOutput(i, cmd);
        }

        public void AddInput(int idx, VideoCommand cmd)
        {
            if(_fromOutName != null)
            {
                cmd.Args.Add("-i");
                cmd.Args.Add($"http://localhost:8080/api/Detection/{ConversionUtils.ToStringInvariant(_minScore)}");
            }
        }

        public override string ComputeFilterComplexOutput()
        {
            if (_fromOutName == null || _toOutName == null || _toJpgName == null) return "";
            var outVideo = this.InStreams[0];
            var outJpgFilter = "fps=1/10,crop=min(iw\\,ih):min(iw\\,ih):(iw-min(iw\\,ih))/2:(ih-min(iw\\,ih))/2,scale=256:256";

            //TODO: overlay using fromOutName
            var overlayFilter = "null";
            return $"[{outVideo}]split[{outVideo}:0],{outJpgFilter}[{_toJpgName}];[{outVideo}:0]{overlayFilter}[{_toOutName}]";
        }

        public WrappedWithCheckOutFilter(string key, FilterGraphItem item): base(key)
        {
            if (!item.Type.Equals(FilterName)) throw new FilterException("Filter name is not matching");
            if (item.In.Count != 2) throw new FilterException("Need 2 inputs in OutFilter: [{video}, {audio}]");
            var outVideo = item.In[0];
            item.Properties.TryGetValue("min_score", out object minScoreObj);
            var minScore = ConversionUtils.EnsureInt(minScoreObj);
            _minScore = (uint)(minScore > 0 ? minScore : 100);
            _fromOutName = outVideo == null ? null : outVideo + ":1";
            _toOutName = outVideo == null ? null : outVideo + ":2";
            _toJpgName = outVideo == null ? null : outVideo + ":3";
            _out = new OutFilter(key, new FilterGraphItem { In = {_toOutName, item.In[1] }, Out = item.Out, Type = "_OUT", Properties = item.Properties });
        }
    }
}
