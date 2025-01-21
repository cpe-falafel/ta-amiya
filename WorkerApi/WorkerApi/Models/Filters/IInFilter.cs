using WorkerApi.Models.Graph;

namespace WorkerApi.Models.Filters
{
    public interface IInFilter
    {

        public void AddInput(int idx, VideoCommand cmd);
    }
}
