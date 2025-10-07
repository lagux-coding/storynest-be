using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Enums;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly ICurrentUserService _currentUserService;
        private readonly INotificationService _notificationService;

        public UserController(IUserService userService, ICurrentUserService currentUserService, INotificationService notificationService)
        {
            _userService = userService;
            _currentUserService = currentUserService;
            _notificationService = notificationService;
        }

        [Authorize]
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
    }
}
