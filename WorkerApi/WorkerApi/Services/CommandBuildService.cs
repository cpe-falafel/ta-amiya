﻿using System.Text;

namespace WorkerApi.Services
{
    public class CommandBuildService : ICommandBuildService
    {
        private readonly IFilterGraphService _filterGraphService;
        private readonly ILogger<ICommandBuildService> _logger;

        public CommandBuildService(IFilterGraphService filterGraphService, ILogger<ICommandBuildService> logger)
        {
            _filterGraphService = filterGraphService;
            _logger = logger;
        }
        public string BuildCommand(string jsonWorkerConfiguration)
        {
            try
            {
                var graph = _filterGraphService.ConvertToGraph(jsonWorkerConfiguration);
                var command = new StringBuilder("ffmpeg");

                var test = graph;

                return command.ToString();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error building ffmpeg command");
                throw;
            }
        }
    }
}
