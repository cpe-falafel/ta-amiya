using WorkerApi.Models.Graph;

namespace WorkerApi.Models.Filters
{
    public abstract class AbstractFilterComplexVertex : FilterVertex
    {
        protected AbstractFilterComplexVertex(string key) : base(key)
        {
        }

        public abstract string ComputeFilterComplexOutput();
    }
}
