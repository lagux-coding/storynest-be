using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;

namespace StoryNest.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class TagController : ControllerBase
    {
        private readonly ITagService _tagService;

        public TagController(ITagService tagService)
        {
            _tagService = tagService;
        }

        [HttpGet]
        public async Task<ActionResult<ApiResponse<object>>> GetAllSystemTag()
        {
            var result = await _tagService.GetAllSystemTagsAsync();
            if (result == null)
                return BadRequest(ApiResponse<object>.Fail("No tags found"));

            return Ok(ApiResponse<object>.Success(result, "Tags retrieved successfully"));  
        }
    }
}
