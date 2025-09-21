using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class CommentController : ControllerBase
    {
        private readonly ICommentService _commentService;
        private readonly ICurrentUserService _currentUserService;

        public CommentController(ICommentService commentService, ICurrentUserService currentUserService)
        {
            _commentService = commentService;
            _currentUserService = currentUserService;
        }

        [Authorize]
        [HttpPost("comment/{storyId}")]
        public async Task<ActionResult<ApiResponse<object>>> Comment(int storyId, [FromBody] CreateCommentRequest request)
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

            var result = await _commentService.AddCommentAsync(request, storyId, userId.Value);
            if (result == null)
                return BadRequest(ApiResponse<object>.Fail(null, "Failed to comment the story"));

            return Ok(ApiResponse<object>.Success(result, "Story commented"));
        }

        [HttpGet("all-comment/{storyId}")]
        public async Task<ActionResult<ApiResponse<object>>> GetAllComments(int storyId, int? parentId, int limit = 10, int offset = 0)
        {
            var comments = await _commentService.GetCommentsAsync(storyId, parentId, limit, offset);
            return Ok(ApiResponse<object>.Success(comments, "Get comments successfully"));
        }
    }
}
