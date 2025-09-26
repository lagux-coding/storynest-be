using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Interfaces;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PlanController : ControllerBase
    {
        private readonly IPlanService _planService;

        public PlanController(IPlanService planService)
        {
            _planService = planService;
        }

        [HttpPost("create")]
        public async Task<ActionResult<ApiResponse<object>>> CreatePlan([FromBody] CreatePlanRequest request)
        {
            var result = await _planService.AddPlanAsync(request);

            if (result > 0)
            {
                return Ok(ApiResponse<object>.Success(result, "Plan created successfully"));
            }
            else
            {
                return BadRequest(ApiResponse<object>.Fail("Failed to create plan"));
            }
        }
    }
}
