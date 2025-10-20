using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Interfaces;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IStorySentimentAnalysisService _storySentimentService;
        private readonly ICurrentUserService _currentUserService;

        public AdminController(IStorySentimentAnalysisService storySentimentService, ICurrentUserService currentUserService)
        {
            _storySentimentService = storySentimentService;
            _currentUserService = currentUserService;
        }

        [HttpGet("report/analysis")]
        public async Task<ActionResult<ApiResponse<object>>> GetAllAnalysis()
        {
            try
            {
                var type = _currentUserService.Type;
                if (type != "admin")
                    return Unauthorized(ApiResponse<object>.Fail("Access denied"));

                var result = await _storySentimentService.GetAllAnalysisAsync();
                return Ok(ApiResponse<object>.Success(result, "Get all analysis successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}
