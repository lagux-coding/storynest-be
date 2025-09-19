using FluentValidation;
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
        private readonly IValidator<CreateStoryRequest> _createStoryValidator;

        public StoryController(IStoryService storyService, ICurrentUserService currentUserService, IValidator<CreateStoryRequest> createStoryValidator)
        {
            _storyService = storyService;
            _currentUserService = currentUserService;
            _createStoryValidator = createStoryValidator;
        }

        [HttpGet("get-stories")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<StoryResponse>>>> GetPreviewStories([FromQuery] int limit = 10, [FromQuery] DateTime? cursor = null)
        {
            var result = await _storyService.GetStoriesPreviewAsync(limit, cursor);
            return Ok(ApiResponse<PaginatedResponse<StoryResponse>>.Success(result));
        }

        [HttpGet("get-by-id-or-slug")]
        public async Task<ActionResult<ApiResponse<object>>> GetStoryByIdOrSlug(int? id, string? slug)
        {
            var result = await _storyService.GetStoryByIdOrSlugAsync(id, slug);
            if (result == null)
                return NotFound(ApiResponse<object>.Fail("Story not found"));
            return Ok(ApiResponse<object>.Success(result));
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

        [Authorize]
        [HttpPut("update/{storyId}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateStory([FromBody] CreateStoryRequest request, int storyId)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
                return BadRequest(ApiResponse<object>.Fail("Authentication failed"));

            var validationResult = await _createStoryValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).Distinct().ToArray()
                    );
                return BadRequest(ApiResponse<object>.Fail(errors, "Validation errors"));
            }

            var result = await _storyService.UpdateStoryAsync(request, userId.Value, storyId);
            return result > 0
                ? Ok(ApiResponse<object>.Success(result, "Story updated successfully"))
                : BadRequest(ApiResponse<object>.Fail("Failed to update story"));
        }
    }
}
