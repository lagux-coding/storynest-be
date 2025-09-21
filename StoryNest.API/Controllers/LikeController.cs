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

        [HttpPost("like")]
        public async Task<ActionResult<ApiResponse<object>>> Like([FromQuery] int storyId)
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

        [HttpPost("unlike")]
        public async Task<ActionResult<ApiResponse<object>>> Unlike([FromQuery] int storyId)
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

            var unLike = await _likeService.UnlikeStoryAsync(storyId, userId.Value);
            if (unLike <= 0)
            {
                return BadRequest(ApiResponse<object>.Fail(null, "Failed to unlike the story"));
            }

            return Ok(ApiResponse<object>.Success(unLike, "Story unliked"));
        }
    }
}
