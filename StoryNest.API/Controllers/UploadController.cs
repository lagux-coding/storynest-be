using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Interfaces;
using StoryNest.Infrastructure.Services.S3;

namespace StoryNest.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class UploadController : ControllerBase
    {
        private readonly ICurrentUserService _currentUserService;
        private readonly IS3Service _s3Service;
        private readonly IUploadService _uploadService;
        private readonly IValidator<UploadMediaRequest> _validator;

        public UploadController(IS3Service s3Service, IValidator<UploadMediaRequest> validator, ICurrentUserService currentUserService, IUploadService uploadService)
        {
            _s3Service = s3Service;
            _validator = validator;
            _currentUserService = currentUserService;
            _uploadService = uploadService;
        }

        [HttpPost("presign-upload")]
        public async Task<ActionResult<ApiResponse<object>>> PresignUrl([FromBody] UploadMediaRequest request)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
                return BadRequest(ApiResponse<object>.Fail("Authentication failed"));
            var result = await _validator.ValidateAsync(request);

            if (!result.IsValid)
            {
                return BadRequest(new
                {
                    errors = result.Errors.Select(e => new { e.PropertyName, e.ErrorMessage })
                });
            }

            var response = await _uploadService.UploadMedia(request, userId.Value);

            return Ok(ApiResponse<object>.Success(response, "Upload image successfully"));
        }

        [HttpPost("confirm-upload")]
        public async Task<ActionResult<ApiResponse<object>>> ConfirmUpload([FromBody] ConfirmUploadRequest request)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
                return BadRequest(ApiResponse<object>.Fail("Authentication failed"));

            var result = await _uploadService.ConfirmUpload(request, userId.Value);
            if (!result)
                return BadRequest(ApiResponse<object>.Fail("Confirm upload failed"));
            return Ok(ApiResponse<object>.Success(null, "Confirm upload successfully"));
        }
    }
}
