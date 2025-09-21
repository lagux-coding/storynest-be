using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Interfaces;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class LikeController : ControllerBase
    {
        private readonly ILikeService _likeService;
        private readonly ICurrentUserService _currentUserService;

        public LikeController(ILikeService likeService, ICurrentUserService currentUserService)
        {
            _likeService = likeService;
            _currentUserService = currentUserService;
        }

        [HttpPost("toggle")]
        public async Task<ActionResult<ApiResponse<object>>> ToggleLike([FromQuery] int storyId)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
            {
                return Unauthorized(new ApiResponse<object>
                {
                    Message = "User not authenticated.",
                    Status = StatusCodes.Status401Unauthorized,
                });
            }

            var like = await _likeService.LikeStoryAsync(storyId, userId.Value);
            if (like <= 0)
            {
                return BadRequest(ApiResponse<object>.Fail(null, "Failed to like the story"));
            }

            return Ok(ApiResponse<object>.Success(like, "Story liked"));
        }
    }
}
