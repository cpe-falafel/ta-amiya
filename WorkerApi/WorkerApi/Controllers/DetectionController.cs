using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Memory;
using System.IO;
using WorkerApi.Services;

namespace YourNamespace.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class DetectionController : ControllerBase
    {

        private readonly string? _pngsDir;
        private readonly ICachedScorerService _scorer;

        private FileContentResult ReadPng(String filename)
        {
            var fileBytes = System.IO.File.ReadAllBytes(Path.Combine(_pngsDir, filename));
            return File(fileBytes, "image/png");
        }

        public DetectionController(ICachedScorerService scorer)
        {
            _pngsDir = Environment.GetEnvironmentVariable("AMIYA_PNGS_DIR");
            _scorer = scorer;
        }


        [HttpGet("{minScore}")]
        public IActionResult GetImage(int minScore)
        {
            if (_pngsDir == null)
            {
                return NotFound();
            }
            uint score = _scorer.GetCachedScore();

            //png name answer the question: "is score below ?"
            return ReadPng(score > minScore ? "no.png" : "yes.png");
        }
    }
}
