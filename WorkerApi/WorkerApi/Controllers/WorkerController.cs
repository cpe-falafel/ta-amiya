using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using WorkerApi.Models.DTO;
using WorkerApi.Services;

namespace WorkerApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class WorkerController : ControllerBase
    {
        private readonly ICommandBuildService _commandBuildService;
        private readonly FfmpegRunnerService _ffmpegRunnerService;
        private readonly ICachedScorerService _cachedScorerService;

        public WorkerController(ICommandBuildService commandBuildService, FfmpegRunnerService ffmpegRunnerService, ICachedScorerService cachedScorerService)
        {
            _cachedScorerService = cachedScorerService;
            _commandBuildService = commandBuildService;
            _ffmpegRunnerService = ffmpegRunnerService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Worker API is running");
        }

        [HttpPost(Name = "UpdateWorkerConfiguration")]
        public IActionResult UpdateWorkerConfiguration([FromBody] UpdateWorkerConfigurationDto updateWorkerConfigurationDto)
        {
            try
            {
                var command = _commandBuildService.BuildCommand(updateWorkerConfigurationDto.JsonWorkerConfiguration);
                //var commandTest = "ffmpeg -i rtmp://localhost:1935/live/test -c copy -f flv rtmp://localhost:1935/input/test\r\n";

                // run ffmpeg command :
                if (command != null && command.Args.Count > 0)
                {
                    _cachedScorerService.SetMinScore(command.MinScore);
                    Task _ = _ffmpegRunnerService.RunFfmpegCommandAsync(command);
                }

                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("stop", Name = "StopWorker")]
        public IActionResult StopWorker()
        {
            try
            {
                _ffmpegRunnerService.StopAllFfmpegProcesses();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
