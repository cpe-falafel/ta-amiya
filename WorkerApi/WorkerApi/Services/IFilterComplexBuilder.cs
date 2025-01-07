using WorkerApi.Models.Graph;

namespace WorkerApi.Services
{
    public interface IFilterComplexBuilder
    {
        void AddFilter(FilterVertex filter);
    }
}
