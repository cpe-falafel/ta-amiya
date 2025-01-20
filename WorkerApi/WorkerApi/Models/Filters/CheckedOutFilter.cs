using System.Collections.Generic;
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

        private readonly string? _toOutName;
        private readonly string? _toJpgName;
        private int? _inPipeIdx;



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
            if(_toJpgName != null)
            {
                _inPipeIdx = idx;
                cmd.Args.Add("-f");
                cmd.Args.Add("image2pipe");
                cmd.Args.Add("-i");
                cmd.Args.Add("-");
                cmd.Args.Add("-vcodec");
                cmd.Args.Add("libx264");
            }
        }

        public override string ComputeFilterComplexOutput()
        {
            if (_toOutName == null || _toJpgName == null || _inPipeIdx == null) return "";
            var outVideo = this.InStreams[0];
            var outJpgFilterNoInput = "fps=1/10,crop=min(iw\\,ih):min(iw\\,ih):(iw-min(iw\\,ih))/2:(ih-min(iw\\,ih))/2,scale=256:256";

            var overlayFilter = $"[{_inPipeIdx}][{outVideo}:1]scale=rw:rh[{outVideo}:2];[{outVideo}:0][{outVideo}:2]overlay";
            return $"[{outVideo}]split=3[{outVideo}:0][{outVideo}:1],"+
                $"{outJpgFilterNoInput}[{_toJpgName}];"+
                $"{overlayFilter}[{_toOutName}]";
        }

        public WrappedWithCheckOutFilter(string key, FilterGraphItem item): base(key)
        {
            if (!item.Type.Equals(FilterName)) throw new FilterException("Filter name is not matching");
            if (item.In.Count != 2) throw new FilterException("Need 2 inputs in OutFilter: [{video}, {audio}]");
            var outVideo = item.In[0];
            item.Properties.TryGetValue("min_score", out object minScoreObj);
            var minScore = ConversionUtils.EnsureInt(minScoreObj);
            _minScore = (uint)(minScore > 0 ? minScore : 100);
            _toOutName = outVideo == null ? null : outVideo + ":3";
            _toJpgName = outVideo == null ? null : outVideo + ":4";
            _out = new OutFilter(key, new FilterGraphItem { In = {_toOutName, item.In[1] }, Out = item.Out, Type = "_OUT", Properties = item.Properties });
        }
    }
}
