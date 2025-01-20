using System.Text;
using WorkerApi.Exceptions;
using WorkerApi.Models.Graph;
using WorkerApi.Services;

namespace WorkerApi.Models.Filters
{
    public class WrappedWithCheckOutFilter : AbstractFilterComplexVertex, IInFilter, IOutFilter
    {

        private readonly OutFilter _out;

        private readonly string? _fromOutName;
        private readonly string? _toOutName;



        public override string FilterName => "_CHECKED_OUT";

        public override string[] OutStreams { get { return _out.OutStreams; } }

        public override string[] InStreams { get { return _out.InStreams; } }


        public override object[] GetFilterParams()
        {
            return _out.GetFilterParams();
        }

        public void AddOutput(int i, VideoCommand cmd)
        {
            if (_fromOutName != null)
            {
                cmd.Args.Add("");
            }
            _out.AddOutput(i, cmd);
        }

        public void AddInput(int idx, VideoCommand cmd)
        {
            throw new NotImplementedException();
        }

        public override string ComputeFilterComplexOutput()
        {
            throw new NotImplementedException();
        }

        public WrappedWithCheckOutFilter(string key, FilterGraphItem item): base(key)
        {
            if (!item.Type.Equals(FilterName)) throw new FilterException("Filter name is not matching");
            if (item.In.Count != 2) throw new FilterException("Need 2 inputs in OutFilter: [{video}, {audio}]");
            var outVideo = item.In[0];
            _fromOutName = outVideo == null ? null : outVideo + ":4";
            _toOutName = outVideo == null ? null : outVideo + ":5";
            _out = new OutFilter(key, new FilterGraphItem { In = {_toOutName, item.In[1] }, Out = item.Out, Type = "_OUT", Properties = item.Properties });
        }
    }
}
