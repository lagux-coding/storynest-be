using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Enums;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [Authorize]
    [ApiController]
    public class AdminController : ControllerBase
    {
        private readonly IUserReportService _userReportService;
        private readonly IStorySentimentAnalysisService _storySentimentService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IDashboardService _dashboardService;

        public AdminController(IStorySentimentAnalysisService storySentimentService, ICurrentUserService currentUserService, IUserReportService userReportService, IDashboardService dashboardService)
        {
            _storySentimentService = storySentimentService;
            _currentUserService = currentUserService;
            _userReportService = userReportService;
            _dashboardService = dashboardService;
        }

        [HttpGet("report/analysis")]
        public async Task<ActionResult<ApiResponse<object>>> GetAllAnalysis()
        {
            try
            {
                var type = _currentUserService.Type;
                if (type != "admin")
                    return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Forbbiden("Access denied"));

                var result = await _storySentimentService.GetAllAnalysisAsync();
                return Ok(ApiResponse<object>.Success(result, "Get all analysis successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("report")]
        public async Task<ActionResult<ApiResponse<object>>> GetReports([FromQuery] ReportType type = ReportType.Story, int page = 1, int pageSize = 10)
        {
            try
            {
                var typeUser = _currentUserService.Type;
                if (typeUser != "admin")
                    return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Forbbiden("Access denied"));

                var result = await _userReportService.GetAllUserReport(type, page, pageSize);
                return Ok(ApiResponse<object>.Success(result, "Get all report base type successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("dashboard")]
        public async Task<ActionResult<ApiResponse<object>>> DashboardStats()
        {
            try
            {
                var typeUser = _currentUserService.Type;
                if (typeUser != "admin")
                    return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Forbbiden("Access denied"));

                var result = await _dashboardService.GetDashboardStatsAsync();
                return Ok(ApiResponse<object>.Success(result, "Get dashboard stats successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("subscription")]
        public async Task<ActionResult<ApiResponse<object>>> SubscriptionStats()
        {
            try
            {
                var typeUser = _currentUserService.Type;
                if (typeUser != "admin")
                    return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Forbbiden("Access denied"));
                var result = await _dashboardService.GetSubscriptionStatsAsync();
                return Ok(ApiResponse<object>.Success(result, "Get subscription stats successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }

        [HttpGet("story")]
        public async Task<ActionResult<ApiResponse<object>>> StoryStats([FromQuery] int page = 1, [FromQuery] int pageSize = 10)
        {
            try
            {
                var typeUser = _currentUserService.Type;
                if (typeUser != "admin")
                    return StatusCode(StatusCodes.Status403Forbidden, ApiResponse<object>.Forbbiden("Access denied"));
                var result = await _dashboardService.GetDashboardStoryAsync(page, pageSize);
                return Ok(ApiResponse<object>.Success(result, "Get story stats successfully"));
            }
            catch (Exception ex)
            {
                return BadRequest(ApiResponse<object>.Fail(ex.Message));
            }
        }
    }
}
