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
        public WorkerController(ICommandBuildService commandBuildService)
        {
            _commandBuildService = commandBuildService;
        }

        [HttpGet]
        public IActionResult Get()
        {
            return Ok("Worker API is running");
        }

        [HttpPost(Name = "GetConfiguration")]
        public IActionResult GetConfiguration([FromBody] UpdateWorkerConfigurationDto updateWorkerConfigurationDto)
        {
            try
            {
                var command = _commandBuildService.BuildCommand(updateWorkerConfigurationDto.JsonWorkerConfiguration);
                return Ok(command);
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }
    }
}
