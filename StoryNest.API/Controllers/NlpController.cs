using AngleSharp.Io;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Dtos.Dto;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Interfaces;
using StoryNest.Shared.Common.Utils;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
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
            List<List<TokenDto>> result = await _nlpService.AnalyzeTextAsync(text);
            return Ok(result);
        }

        [HttpPost("check")]
        public async Task<ActionResult<ApiResponse<object>>> CheckOffensive(CheckOffensiveRequest request)
        {
            try
            {
                request.Title = TextNormalizer.Normalize(request.Title, true);
                request.Content = TextNormalizer.Normalize(request.Content, true);
                for (int i = 0; i < request.Tags.Count; i++)
                {
                    request.Tags[i] = TextNormalizer.Normalize(request.Tags[i], true);
                }

                var result = await _nlpService.CompareOffensiveAsync(request);
                if (result.Count > 0)
                {
                    return BadRequest(ApiResponse<object>.Fail(result, "Offensive content detected"));
                }
                else
                {
                    return Ok(ApiResponse<object>.Success(result, "No offensive content detected"));
                }
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}
