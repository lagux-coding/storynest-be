using OpenAI.Images;
using StoryNest.Application.Interfaces;
using Microsoft.Extensions.Configuration;

namespace StoryNest.Infrastructure.Services.OpenAI
{
    public class OpenAIService : IOpenAIService
    {
        private readonly IConfiguration _configuration;
        private readonly IAICreditService _aiCredit;
        private readonly ImageClient _imageClient;
        private readonly IS3Service _s3Service;
        private readonly IUnitOfWork _unitOfWork;

        public OpenAIService(IConfiguration configuration, IAICreditService aiCredit, ImageClient imageClient, IS3Service s3Service, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _aiCredit = aiCredit;
            _imageClient = imageClient;
            _s3Service = s3Service;
            _unitOfWork = unitOfWork;
        }

        public async Task<string> GenerateImageAsync(string content, long userId)
        {
            // Check user credits
            var userCredit = await _aiCredit.GetUserCredit(userId);
            if (userCredit == null || userCredit.TotalCredits <= 0)
            {
                throw new Exception("Insufficient AI credits.");
            }

            // Generate image using OpenAI
            string basePrompt = "Anime illustration style with soft pastel colors, clean lines, and expressive unisex characters. " 
                + "Blend 2D and 3D effects for depth, with a warm and creative atmosphere. " 
                + "The image should feel light, inspiring, and imaginative, evoking the spirit of personal storytelling. " 
                + "Backgrounds may include cozy or abstract elements that symbolize memory, creativity, and connection. " 
                + "Overall tone: safe, positive, whimsical, and welcoming — perfectly suited for the StoryNest community.";

            string prompt = $"{basePrompt} Nội dung truyện: {content}";

            ImageGenerationOptions options = new()
            {
                Quality = GeneratedImageQuality.Standard,
                Size = GeneratedImageSize.W1024xH1024,
                ResponseFormat = GeneratedImageFormat.Bytes,
            };
            GeneratedImage image = await _imageClient.GenerateImageAsync(prompt, options);
            BinaryData bytes = image.ImageBytes;

            var ms = new MemoryStream(bytes.ToArray());
            var key = await _s3Service.UploadAIImage(ms);
            if (string.IsNullOrEmpty(key))
                throw new Exception("Failed to upload image.");

            // Deduct user credit
            userCredit.TotalCredits -= 1;
            userCredit.UsedCredits += 1;
            if (userCredit.TotalCredits < 0)
                userCredit.TotalCredits = 0;
            await _aiCredit.UpdateCreditsAsync(userCredit);
            await _unitOfWork.SaveAsync();

            var cdnDomain = _configuration["CDN_DOMAIN"];

            return $"{cdnDomain}/{key}";
        }
    }
}
