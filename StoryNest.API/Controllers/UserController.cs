using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Enums;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;
        private readonly IUserMediaService _userMediaService;
        private readonly IUserReportService _reportService;

        public UserController(IUserService userService, ICurrentUserService currentUserService, INotificationService notificationService, IUserMediaService userMediaService, IUserReportService reportService)
        {
            _userService = userService;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
            _userMediaService = userMediaService;
            _reportService = reportService;
        }


        [HttpGet("me")]
        public async Task<ActionResult<ApiResponse<object>>> GetMe()
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));
            }

            var result = await _userService.GetMe(userId.Value);
            return Ok(ApiResponse<object>.Success(result, "Get profile successfully"));
        }

        [HttpPut("update")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateProfile([FromBody] UpdateUserProfileRequest request)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));
                }
                var result = await _userService.UpdateUserProfille(userId.Value, request);
                return Ok(ApiResponse<object>.Success(result, "Update profile successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail($"Update failed: {ex.Message}"));
            }
        }

        [HttpGet("media")]
        public async Task<ActionResult<ApiResponse<object>>> GetUserMedias([FromQuery] MediaType? type = null)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));
            }
            var result = await _userMediaService.GetAllMediaByUser(userId.Value, type);
            return Ok(ApiResponse<List<UserMediaResponse>>.Success(result, "Get user media successfully"));
        }

        [HttpGet("comment")]
        public async Task<ActionResult<ApiResponse<object>>> GetUserComments([FromQuery] int limit = 10, [FromQuery] int cursor = 0)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));
                }
                var result = await _userService.GetUserByIdAsync(userId.Value);
                if (result == null)
                {
                    return NotFound(ApiResponse<object>.Fail("User not found"));
                }
                var comments = await _userService.GetUserCommentAsync(userId.Value, limit, cursor);
                return Ok(ApiResponse<object>.Success(comments, "Get user comments successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("comment/story")]
        public async Task<ActionResult<ApiResponse<object>>> GetStoryByComment([FromQuery] int commentId)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));
                }
                var result = await _userService.GetStoryByCommentAsync(commentId);
                if (result == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Story not found"));
                }
                return Ok(ApiResponse<object>.Success(result, "Get story by comment successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpPost("report")]
        public async Task<ActionResult<ApiResponse<object>>> ReportUser([FromBody] UserReportRequest request, [FromQuery] int storyId, [FromQuery] int commentId = 0)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));
                }
                var checkUser = await _userService.GetUserByIdAsync(userId.Value);
                if (checkUser == null)
                {
                    return NotFound(ApiResponse<object>.Fail("Reported user not found"));
                }

                var result = await _reportService.CreateReportAsync(request, userId.Value, storyId, commentId);

                if (result < 0)
                {
                    return BadRequest(ApiResponse<object>.Fail("Send report failed"));
                }
                return Ok(ApiResponse<object>.Success(result, "Send report successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail($"Report failed: {ex.Message}"));
            }
        }
    }
}
