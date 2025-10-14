using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Interfaces;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class StoryViewController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IStoryViewService _storyViewService;
        private readonly IUnitOfWork _unitOfWork;

        public StoryViewController(ICurrentUserService currentUserService, IStoryViewService storyViewService, IUnitOfWork unitOfWork)
        {
            _currentUserService = currentUserService;
            _storyViewService = storyViewService;
            _unitOfWork = unitOfWork;
        }

        [HttpPost("view")]
        public async Task<ActionResult<ApiResponse<object>>> LogStoryView([FromQuery] int storyId)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
                return Unauthorized(ApiResponse<string>.Fail("User not logged in"));

            var ip = HttpContext.Connection.RemoteIpAddress?.ToString();
            var device = Request.Headers["User-Agent"].ToString();

            await _storyViewService.LogStoryViewAsync(storyId, userId.Value, ip, device);
            await _unitOfWork.SaveAsync();
            return Ok(ApiResponse<string>.Success("View logged"));
        }

    }
}
