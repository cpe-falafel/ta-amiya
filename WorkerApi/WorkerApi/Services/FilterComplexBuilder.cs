using System.Text;
using WorkerApi.Models.Graph;

namespace WorkerApi.Services
{
    public class FilterComplexBuilder : IFilterComplexBuilder
    {
        private readonly ILogger<IFilterComplexBuilder> _logger;
        private List<FilterVertex> _filters;


        public FilterComplexBuilder(ILogger<IFilterComplexBuilder> logger)
        {
            _logger = logger;
            _filters = new List<FilterVertex>();
        }

        public void AddFilter(FilterVertex filter)
        {
            try
            {
                _filters.Add(filter);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding filter to filter complex");
                throw;
            }
        }

        public string BuildFilterComplex(FilterVertex filter)
        {
            try
            {
                var filterComplex = new StringBuilder();
                
                // TODO

                return filterComplex.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building filter complex");
                throw;
            }
        }
    }
}
