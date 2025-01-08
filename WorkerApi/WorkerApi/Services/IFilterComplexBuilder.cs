using WorkerApi.Models.Filters;
using WorkerApi.Models.Graph;

namespace WorkerApi.Services
{
    public interface IFilterComplexBuilder
    {
        void AddInput(FilterVertex input);
        void AddFilter(AbstractFilterComplexVertex filter);
        void AddOutput(FilterVertex output);
        string BuildFilterComplex();
    }
}
