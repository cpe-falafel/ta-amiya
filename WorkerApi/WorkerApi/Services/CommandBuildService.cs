using QuickGraph;
using WorkerApi.Models;
using WorkerApi.Models.Filters;
using WorkerApi.Models.Graph;

namespace WorkerApi.Services
{
    public class CommandBuildService : ICommandBuildService
    {
        private readonly IFilterGraphService _filterGraphService;
        private readonly ILogger<ICommandBuildService> _logger;
        private readonly ILoggerFactory _loggerFactory;

        public CommandBuildService(IFilterGraphService filterGraphService, ILogger<ICommandBuildService> logger, ILoggerFactory loggerFactory)
        {
            _filterGraphService = filterGraphService;
            _logger = logger;
            _loggerFactory = loggerFactory;
        }

        private List<T> CastWhere<T>(List<FilterVertex> vertices) where T : FilterVertex
        {
            return vertices.Where(v => v is T).Select(v => (T)v).ToList();
        }

        public VideoCommand BuildCommand(string jsonWorkerConfiguration)
        {
            try
            {
                var cmd = new VideoCommand();
                var graph = _filterGraphService.ConvertToGraph(jsonWorkerConfiguration);

                AddInputs(cmd, graph);

                AddFilters(cmd, graph);

                AddOutputs(cmd, graph);

                return cmd;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building ffmpeg command");
                throw;
            }
        }

        private void AddFilters(VideoCommand cmd, BidirectionalGraph<FilterVertex, StreamEdge> graph)
        {
            IFilterComplexBuilder filterComplexBuilder = new FilterComplexBuilder(_loggerFactory.CreateLogger<FilterComplexBuilder>());
            var filterComplexes = CastWhere<AbstractFilterComplexVertex>(graph.Vertices.ToList());
            foreach (var filter in filterComplexes)
            {
                filterComplexBuilder.AddFilter(filter);
            }
            cmd.Args.Append(" -filter_complex");
            cmd.Args.Append(" \"$AMIYA_FC\"");
            cmd.Env.Add("AMIYA_FC", filterComplexBuilder.BuildFilterComplex());
        }

        private void AddInputs(VideoCommand cmd, BidirectionalGraph<FilterVertex, StreamEdge> graph)
        {
            int inCount = 0;
            foreach (var inputNode in CastWhere<InFilter>(graph.Vertices.ToList()))
            {
                inputNode.AddInput(inCount, cmd);
                inCount++;
            }
        }

        private void AddOutputs(VideoCommand cmd, BidirectionalGraph<FilterVertex, StreamEdge> graph)
        {
            int outCount = 0;
            foreach (var inputNode in CastWhere<OutFilter>(graph.Vertices.ToList()))
            {
                inputNode.AddOutput(outCount, cmd);
                outCount++;
            }
        }
    }
}