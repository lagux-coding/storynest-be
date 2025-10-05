using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Interfaces;
using StoryNest.Shared.Common.Utils;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoryController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IStoryService _storyService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IValidator<CreateStoryRequest> _createStoryValidator;

        public StoryController(IStoryService storyService, ICurrentUserService currentUserService, IValidator<CreateStoryRequest> createStoryValidator, IConfiguration configuration)
        {
            _storyService = storyService;
            _currentUserService = currentUserService;
            _createStoryValidator = createStoryValidator;
            _configuration = configuration;
        }

        [HttpGet("get-stories")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<StoryResponse>>>> GetPreviewStories([FromQuery] int limit = 10, [FromQuery] DateTime? cursor = null)
        {
            PaginatedResponse<StoryResponse> result = new();
            var userId = _currentUserService.UserId;   
            
            if (userId != null)
                result = await _storyService.GetStoriesPreviewAsync(limit, cursor, userId.Value);
            else
                result = await _storyService.GetStoriesPreviewAsync(limit, cursor);

            return Ok(ApiResponse<PaginatedResponse<StoryResponse>>.Success(result));
        }

        [HttpGet("search")]
        public async Task<ActionResult<ApiResponse<object>>> SearchStories([FromQuery] string? keyword, [FromQuery] int limit = 20, [FromQuery] int? lastId = null)
        {
            var result = await _storyService.SearchStoriesAsync(keyword, limit, lastId);
            return Ok(ApiResponse<object>.Success(result, "Search completed"));
        }

        [HttpGet("get-by-user")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<StoryResponse>>>> GetStoriesByUser([FromQuery] long userId, [FromQuery] int limit = 10, [FromQuery] DateTime? cursor = null)
        {
            PaginatedResponse<GetStoryResponse> result = new();
            var currentUserId = _currentUserService.UserId;
            if (currentUserId != null)
                result = await _storyService.GetStoriesByUserAsync(userId, cursor, currentUserId.Value);
            else
                result = await _storyService.GetStoriesByUserAsync(userId, cursor);
            return Ok(ApiResponse<PaginatedResponse<GetStoryResponse>>.Success(result));
        }

        [HttpGet("get-by-owner")]
        public async Task<ActionResult<ApiResponse<object>>> GetStoriesByOwner(
                    [FromQuery] int limit = 10,
                    [FromQuery] DateTime? cursor = null)
        {
            var currentUserId = _currentUserService.UserId;

            if (currentUserId == null)
                return Unauthorized(ApiResponse<object>.Fail("Authentication failed"));

            var result = await _storyService.GetStoriesByOwnerAsync(
                currentUserId.Value,
                cursor,
                currentUserId.Value
            );

            return Ok(ApiResponse<PaginatedResponse<GetStoryResponse>>.Success(result));
        }

        [HttpGet("get-by-user-ai")]
        public async Task<ActionResult<ApiResponse<PaginatedResponse<StoryResponse>>>> GetStoriesByUserAI([FromQuery] long userId, [FromQuery] int limit = 10, [FromQuery] DateTime? cursor = null)
        {
            PaginatedResponse<GetStoryResponse> result = new();
            var currentUserId = _currentUserService.UserId;
            if (currentUserId != null)
                result = await _storyService.GetStoriesByUserAIAsync(userId, cursor, currentUserId.Value);
            else
                result = await _storyService.GetStoriesByUserAIAsync(userId, cursor);
            return Ok(ApiResponse<PaginatedResponse<GetStoryResponse>>.Success(result));
        }

        [HttpGet("get-by-owner-ai")]
        public async Task<ActionResult<ApiResponse<object>>> GetStoriesByOwnerAI(
                    [FromQuery] int limit = 10,
                    [FromQuery] DateTime? cursor = null)
        {
            var currentUserId = _currentUserService.UserId;

            if (currentUserId == null)
                return Unauthorized(ApiResponse<object>.Fail("Authentication failed"));

            var result = await _storyService.GetStoriesByOwnerAIAsync(
                currentUserId.Value,
                cursor,
                currentUserId.Value
            );

            return Ok(ApiResponse<PaginatedResponse<GetStoryResponse>>.Success(result));
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
            if (storyId < 0)
            {
                return BadRequest(ApiResponse<object>.Fail("Failed to create story"));
            }
            else if (storyId > 0 && request.IsAnonymous)
            {
                var username = UsernameGenerateHelperHelper.GenerateAnonymousName(storyId);
                var avatarUrl = "system-assets/anonymous-avatar.webp";

                return Ok(ApiResponse<object>.Success(new { StoryId = storyId, Username = username, AvatarUrl = avatarUrl }));
            }

            return Ok(ApiResponse<object>.Success(new { StoryId = storyId }));
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

            if (result < 0)
            {
                return BadRequest(ApiResponse<object>.Fail("Failed to update story"));
            }
            else if (result > 0 && request.IsAnonymous)
            {
                var username = UsernameGenerateHelperHelper.GenerateAnonymousName(storyId);
                var avatarUrl = "system-assets/anonymous-avatar.webp";

                return Ok(ApiResponse<object>.Success(new { StoryId = storyId, Username = username, AvatarUrl = avatarUrl }));
            }

            return Ok(ApiResponse<object>.Success(new { StoryId = storyId }));
        }

        [Authorize]
        [HttpDelete("delete/{storyId}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteStory(int storyId)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
                return BadRequest(ApiResponse<object>.Fail("Authentication failed"));

            var result = await _storyService.RemoveStoryAsync(storyId, userId.Value);
            return result > 0
                ? Ok(ApiResponse<object>.Success(result, "Story deleted successfully"))
                : BadRequest(ApiResponse<object>.Fail("Failed to delete story"));
        }
    }
}
