using System.Text;
using WorkerApi.Models.Filters;

namespace WorkerApi.Services
{
    public class FilterComplexBuilder : IFilterComplexBuilder
    {
        private readonly ILogger<IFilterComplexBuilder> _logger;
        private List<String> _filters;

        public FilterComplexBuilder(ILogger<IFilterComplexBuilder> logger)
        {
            _logger = logger;
            _filters = new List<String>();
        }

        public void AddFilter(AbstractFilterComplexVertex filter)
        {
            try
            {
                var filterCommand = filter.ComputeFilterComplexOutput();

                _filters.Add(filterCommand);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding filter to filter complex");
                throw;
            }
        }

        public string BuildFilterComplex()
        {
            var command = new StringBuilder();
            foreach (var f in _filters)
            {
                if (command.Length > 0) command.Append(";");
                command.Append(f);
            }
            return command.ToString();
                
        }
    }
}