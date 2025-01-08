using WorkerApi.Models.Filters;

namespace WorkerApi.Services
{
    public class CommandBuildService : ICommandBuildService
    {
        private readonly IFilterGraphService _filterGraphService;
        private readonly ILogger<ICommandBuildService> _logger;
        private readonly IFilterComplexBuilder _filterComplexBuilder;

        public CommandBuildService(IFilterGraphService filterGraphService, ILogger<ICommandBuildService> logger, IFilterComplexBuilder filterComplexBuilder)
        {
            _filterGraphService = filterGraphService;
            _logger = logger;
            _filterComplexBuilder = filterComplexBuilder;
        }

        public string BuildCommand(string jsonWorkerConfiguration)
        {
            try
            {
                var graph = _filterGraphService.ConvertToGraph(jsonWorkerConfiguration);

                // Traitement des entrées
                var inputNodes = graph.Vertices.Where(v => v.FilterName == "_IN").ToList();
                foreach (var inputNode in inputNodes)
                {
                    _filterComplexBuilder.AddInput(inputNode);
                }

                // Traitement des filtres
                var filterComplexes = graph.Vertices.Where(v => v is AbstractFilterComplexVertex).Select(v => (AbstractFilterComplexVertex)v).ToList();
                foreach (var filter in filterComplexes)
                {
                    _filterComplexBuilder.AddFilter(filter);
                }

                // Traitement des sorties
                var outputNodes = graph.Vertices.Where(v => v.FilterName == "_OUT").ToList();
                foreach (var outputNode in outputNodes)
                {
                    _filterComplexBuilder.AddOutput(outputNode);
                }

                // Construction de la commande finale
                return "ffmpeg" + _filterComplexBuilder.BuildFilterComplex();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building ffmpeg command");
                throw;
            }
        }
    }
}