using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OpenAI.Chat;
using OpenAI.Images;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Interfaces;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AIStoryController : ControllerBase
    {
        private readonly ImageClient _imageClient;
        private readonly IS3Service _s3Service;
        private readonly IOpenAIService _openAIService;
        private readonly ICurrentUserService _currentUserService;

        public AIStoryController(ImageClient imageClient, IS3Service s3Service, IOpenAIService openAIService, ICurrentUserService currentUserService)
        {
            _imageClient = imageClient;
            _s3Service = s3Service;
            _openAIService = openAIService;
            _currentUserService = currentUserService;
        }

        [HttpPost("generate-image")]
        public async Task<ActionResult<ApiResponse<object>>> GenerateImage([FromBody] string content)
        {
            var userId = _currentUserService.UserId;
            if (userId == null)
                return BadRequest(ApiResponse<object>.Fail("Authentication failed"));

            var result = await _openAIService.GenerateImageAsync(content, userId.Value);
            return Ok(ApiResponse<object>.Success(result, "Image generated"));
        }

        [HttpPost("complete")]
        public async Task<IActionResult> CompleteChat([FromBody] string message)
        {
            string basePrompt =
    "Anime illustration style with soft pastel colors, clean lines, and expressive unisex characters. " +
    "Blend 2D and 3D effects for depth, with a warm and creative atmosphere. " +
    "The image should feel light, inspiring, and imaginative, evoking the spirit of personal storytelling. " +
    "Backgrounds may include cozy or abstract elements that symbolize memory, creativity, and connection. " +
    "Overall tone: safe, positive, whimsical, and welcoming — perfectly suited for the StoryNest community.";



            string prompt = $"{basePrompt} Nội dung truyện: {message}";

            ImageGenerationOptions options = new()
            {
                Quality = GeneratedImageQuality.High,
                Size = GeneratedImageSize.W1024xH1024,
                ResponseFormat = GeneratedImageFormat.Bytes,
            };
            GeneratedImage image = await _imageClient.GenerateImageAsync(prompt, options);
            BinaryData bytes = image.ImageBytes;

            var ms = new MemoryStream(bytes.ToArray());

            return Ok(new { response = await _s3Service.UploadAIImage(ms) });
        }
    }
}
