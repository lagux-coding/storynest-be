using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Interfaces;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoryController : ControllerBase
    {
        private readonly IStoryService _storyService;
        private readonly ICurrentUserService _currentUserService;

        public StoryController(IStoryService storyService, ICurrentUserService currentUserService)
        {
            _storyService = storyService;
            _currentUserService = currentUserService;
        }

        [HttpGet("get-stories")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<StoryPreviewResponse>>>> GetPreviewStories([FromQuery] int limit = 10, [FromQuery] DateTime? cursor = null)
        {
            var result = await _storyService.GetStoriesPreviewAsync(limit, cursor);
            return Ok(ApiResponse<PaginatedResponse<StoryPreviewResponse>>.Success(result));
        }

        [Authorize]
        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<object>>> CreateStory([FromBody] CreateStoryRequest request)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
                return BadRequest(ApiResponse<object>.Fail("Authentication failed"));

            if (!ModelState.IsValid)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid request data"));
            }

            var storyId = await _storyService.CreateStoryAsync(request, userId.Value);

            return storyId > 0
                ? Ok(ApiResponse<object>.Success(storyId))
                : BadRequest(ApiResponse<object>.Fail("Failed to create story"));
        }
    }
}
