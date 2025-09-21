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
        [HttpPost("{storyId}")]
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

        [Authorize]
        [HttpPost("update/{storyId}")]
        public async Task<ActionResult<ApiResponse<object>>> UpdateComment(int storyId, int commentId, [FromBody] CreateCommentRequest request)
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
            var result = await _commentService.UpdateCommentAsync(request, commentId, userId.Value);
            if (!result)
                return BadRequest(ApiResponse<object>.Fail(null, "Failed to update the comment"));
            return Ok(ApiResponse<object>.Success(result, "Comment updated"));
        }

        [Authorize]
        [HttpPost("delete/{commentId}")]
        public async Task<ActionResult<ApiResponse<object>>> DeleteComment(int commentId)
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
            var result = await _commentService.DeleteCommentAsync(commentId, userId.Value);
            if (!result)
                return BadRequest(ApiResponse<object>.Fail(null, "Failed to delete the comment"));
            return Ok(ApiResponse<object>.Success(result, "Comment deleted"));
        }

        [HttpGet("all-comment/{storyId}")]
        public async Task<ActionResult<ApiResponse<object>>> GetAllComments(int storyId, int? parentId, int limit = 10, int offset = 0)
        {
            var comments = await _commentService.GetCommentsAsync(storyId, parentId, limit, offset);
            return Ok(ApiResponse<object>.Success(comments, "Get comments successfully"));
        }
    }
}
