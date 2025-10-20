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
        private readonly IGoogleNLPService _googleNlpService;

        public NlpController(IVnCoreNlpService nlpService, IGoogleNLPService googleNlpService)
        {
            _nlpService = nlpService;
            _googleNlpService = googleNlpService;
        }

        [HttpPost("analyze")]
        public async Task<IActionResult> Analyze([FromBody] string text)
        {
            var result = TextNormalizer.NormalizeStoryText(text);
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
