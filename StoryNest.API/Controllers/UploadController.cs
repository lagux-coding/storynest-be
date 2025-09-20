using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Dtos.Request;
using StoryNest.Infrastructure.Services.S3;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly S3Service _s3Service;
        private readonly IValidator<UploadImageRequest> _validator;

        public UploadController(S3Service s3Service, IValidator<UploadImageRequest> validator)
        {
            _s3Service = s3Service;
            _validator = validator;
        }

        [HttpPost("presign-upload")]
        public async Task<ActionResult<ApiResponse<object>>> PresignUrl([FromBody] UploadImageRequest request)
        {
            var result = await _validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                return BadRequest(new
                {
                    errors = result.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                });
            }
            return Ok(new { message = "Presigned URL generated" });
        }
    }
}
