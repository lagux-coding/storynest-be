using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryNest.Application.Interfaces;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class NlpController : ControllerBase
    {
        private readonly IVnCoreNlpService _nlpService;

        public NlpController(IVnCoreNlpService nlpService)
        {
            _nlpService = nlpService;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze([FromBody] string text)
        {
            var result = await _nlpService.AnalyzeTextAsync(text);
            return Ok(result);
        }
    }
}
