using WorkerApi.Models.Graph;

namespace WorkerApi.Models.Filters
{
    public interface IOutFilter
    {

        public void AddOutput(int _, VideoCommand cmd);
    }
}
