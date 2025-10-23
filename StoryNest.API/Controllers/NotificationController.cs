using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Interfaces;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class NotificationController : ControllerBase
    {

        private readonly INotificationService _notificationService;
        private readonly ICurrentUserService _currentUserService;

        public NotificationController(INotificationService notificationService, ICurrentUserService currentUserService)
        {
            _notificationService = notificationService;
            _currentUserService = currentUserService;
        }

        [HttpGet("all")]
        public async Task<ActionResult<ApiResponse<object>>> GetAllNotifications([FromQuery] int limit = 10, [FromQuery] long cursor = 0)
        {
            try
            {
                var userId = _currentUserService.UserId;
                if (userId == null)
                {
                    return Unauthorized(ApiResponse<object>.Fail("Unauthorized"));
                }

                var result = await _notificationService.GetAllNotificationsAsync(userId.Value, limit, cursor);
                return Ok(ApiResponse<object>.Success(result, "Notifications retrieved successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}
