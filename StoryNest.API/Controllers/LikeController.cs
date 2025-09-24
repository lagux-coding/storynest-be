using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Interfaces;

namespace StoryNest.API.Controllers
{
    [Authorize]
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
       
        [HttpPost("like/{storyId}")]
        public async Task<ActionResult<ApiResponse<object>>> Like(int storyId)
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

        [HttpPost("unlike/{storyId}")]
        public async Task<ActionResult<ApiResponse<object>>> Unlike(int storyId)
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

        [HttpGet("all-likes/{storyId}")]
        public async Task<ActionResult<List<UserBasicResponse>>> GetAllUserLike(int storyId)
        {
            var users = await _likeService.GetAllUserLikesAsync(storyId);
            if (users == null || users.Count == 0)
            {
                return NotFound(ApiResponse<object>.Fail(null, "No likes found for the story"));
            }
            return Ok(ApiResponse<List<UserBasicResponse>>.Success(users, "Users retrieved successfully"));
        }
    }
}
