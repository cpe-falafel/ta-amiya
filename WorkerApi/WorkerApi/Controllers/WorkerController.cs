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
        private readonly IApiKeyValidatorService _apiKeyValidatorService;

        public WorkerController(ICommandBuildService commandBuildService, FfmpegRunnerService ffmpegRunnerService, ICachedScorerService cachedScorerService, IApiKeyValidatorService apiKeyValidatorService)
        {
            _cachedScorerService = cachedScorerService;
            _commandBuildService = commandBuildService;
            _ffmpegRunnerService = ffmpegRunnerService;
            _apiKeyValidatorService = apiKeyValidatorService;
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
                if (!Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader) || !_apiKeyValidatorService.IsValid(apiKeyHeader))
                {
                    return Unauthorized(new { error = "Invalid or missing token" });
                }

                var command = _commandBuildService.BuildCommand(updateWorkerConfigurationDto.JsonWorkerConfiguration);

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
                if (!Request.Headers.TryGetValue("X-Api-Key", out var apiKeyHeader) || !_apiKeyValidatorService.IsValid(apiKeyHeader))
                {
                    return Unauthorized(new { error = "Invalid or missing token" });
                }

                _ffmpegRunnerService.StopAllFfmpegProcesses();
                return Ok();
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("status")]
        public async Task<ActionResult<WorkerStatusDto>> GetStatus()
        {
            try
            {
                var status = await _ffmpegRunnerService.GetStatusAsync();
                return Ok(status);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
