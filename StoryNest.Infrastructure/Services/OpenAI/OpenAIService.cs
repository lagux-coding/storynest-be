using Microsoft.Extensions.Configuration;
using OpenAI.Audio;
using OpenAI.Images;
using StoryNest.Application.Interfaces;
using StoryNest.Application.Services;
using StoryNest.Domain.Enums;
using SharpToken;

namespace StoryNest.Infrastructure.Services.OpenAI
{
    public class OpenAIService : IOpenAIService
    {
        private readonly IConfiguration _configuration;
        private readonly IAICreditService _aiCredit;
        private readonly ImageClient _imageClient;
        private readonly AudioClient _audioClient;
        private readonly IS3Service _s3Service;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IUserMediaService _userMediaService;
        private readonly IAITransactionService _aiTransactionService;
        private readonly IAIUsageLogService _aiUsageLogService;

        public OpenAIService(IConfiguration configuration, IAICreditService aiCredit, ImageClient imageClient, IS3Service s3Service, IUnitOfWork unitOfWork, AudioClient audioClient, IUserMediaService userMediaService, IAITransactionService aiTransactionService, IAIUsageLogService aiUsageLogService)
        {
            _configuration = configuration;
            _aiCredit = aiCredit;
            _imageClient = imageClient;
            _s3Service = s3Service;
            _unitOfWork = unitOfWork;
            _audioClient = audioClient;
            _userMediaService = userMediaService;
            _aiTransactionService = aiTransactionService;
            _aiUsageLogService = aiUsageLogService;
        }

        public async Task<string> GenerateAudioAsync(string content, long userId)
        {
            try
            {
                // Check user credits
                var userCredit = await _aiCredit.GetUserCredit(userId);
                if (userCredit == null || userCredit.TotalCredits <= 0)
                {
                    return "Not enough credit";
                }

                // Generate audio using OpenAI
                var options = new SpeechGenerationOptions
                {
#pragma warning disable OPENAI001
                    Instructions = $"Affect: A caring, gentle presence—like a close friend who listens with patience and empathy.\r\n\r\nTone: Soft, soothing, and approachable. Neutral enough to adapt, yet warm enough to comfort.\r\n\r\nDelivery: Smooth and unhurried most of the time, with gentle pauses. But when urgency or excitement is needed, the pace naturally quickens—clear, but never rushed.\r\n\r\nEmotion: Subtle warmth and reassurance, mixed with encouragement. Always calm at the core, but able to add a spark of energy when the moment calls for it.\r\n\r\nPunctuation: Light, flowing sentences with commas guiding a gentle rhythm. Occasionally shorter, sharper sentences to match faster delivery—easy to follow, like natural conversation.",
#pragma warning restore OPENAI001
                    ResponseFormat = GeneratedSpeechFormat.Wav
                };

                var audio = await _audioClient.GenerateSpeechAsync(content, "sage", options);
                BinaryData bytes = audio.Value;
                using var ms = new MemoryStream(bytes.ToArray());

                var key = await _s3Service.UploadAIAudio(ms);
                if (string.IsNullOrEmpty(key))
                    throw new Exception("Failed to upload audio to S3");

                // Add to user media
                var media = await _userMediaService.AddUserMedia(userId, key, MediaType.Audio, UserMediaStatus.Orphaned);

                // Deduct user credit
                userCredit.TotalCredits -= 1;
                userCredit.UsedCredits += 1;

                await _aiCredit.UpdateCreditsAsync(userCredit);

                await _aiTransactionService.AddTransactionAsync(userId, userCredit.Id, 1, "gen audio", AITransactionType.Spent);

                await _unitOfWork.SaveAsync();

                return key;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<string> GenerateImageAsync(string content, long userId)
        {
            try
            {
                // Check user credits
                var userCredit = await _aiCredit.GetUserCredit(userId);
                if (userCredit == null || userCredit.TotalCredits <= 0)
                {
                    throw new InvalidOperationException("Not enough credit");
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

                using var ms = new MemoryStream(bytes.ToArray());
                var key = await _s3Service.UploadAIImage(ms);
                if (string.IsNullOrEmpty(key))
                    throw new Exception("Failed to upload image to S3");

                // Add to user media
                var media = await _userMediaService.AddUserMedia(userId, key, MediaType.Image, UserMediaStatus.Orphaned);

                // Deduct user credit
                userCredit.TotalCredits -= 1;
                userCredit.UsedCredits += 1;

                await _aiCredit.UpdateCreditsAsync(userCredit);

                await _aiTransactionService.AddTransactionAsync(userId, userCredit.Id, 1, "gen image", AITransactionType.Spent);

                await _unitOfWork.SaveAsync();

                return key;
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
