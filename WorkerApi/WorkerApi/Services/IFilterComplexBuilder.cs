using WorkerApi.Models.Filters;

namespace WorkerApi.Services
{
    public interface IFilterComplexBuilder
    {
        void AddFilter(AbstractFilterComplexVertex filter);
        string BuildFilterComplex();
    }
}
