using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace WorkerApi.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class WorkerController : ControllerBase
    {
        public WorkerController()
        {
        }

        [HttpGet]
        // Get json configuration : return Json object
        public IActionResult GetConfiguration()
        {
            var result = false;

            if (result)
            {
                return Ok(new { result = "success" });
            }
            else
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { result = "error" });
            }
        }
    }
}
