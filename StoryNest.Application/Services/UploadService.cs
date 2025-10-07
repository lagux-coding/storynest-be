using AutoMapper;
using HtmlAgilityPack;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Services
{
    public class UploadService : IUploadService
    {
        private readonly IS3Service _s3Service;
        private readonly IUserMediaService _userMediaService;
        private readonly IStoryService _storyService;
        private readonly IMediaService _mediaService;
        private readonly IUserService _userService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public UploadService(IS3Service s3Service, IStoryService storyService, IUserService userService, IUnitOfWork unitOfWork, IMediaService mediaService, IMapper mapper, IUserMediaService userMediaService)
        {
            _s3Service = s3Service;
            _storyService = storyService;
            _userService = userService;
            _unitOfWork = unitOfWork;
            _mediaService = mediaService;
            _mapper = mapper;
            _userMediaService = userMediaService;
        }

        public async Task<bool> ConfirmUpload(ConfirmUploadRequest request, long userId)
        {
            try
            {
                // Check resource type
                switch (request.ResourceType)
                {
                    case "avatar":
                    case "cover-user":
                        var user = await _userService.GetUserByIdAsync(userId);
                        if (user == null)
                            return false;

                        if (request.ResourceType == "avatar")
                            user.AvatarUrl = request.FileKeys[0].ToLower().Trim();
                        else 
                            user.CoverUrl = request.FileKeys[0].ToLower().Trim();

                        await _userService.UpdateUserAsync(user);
                        await _unitOfWork.SaveAsync();

                        foreach (var key in request.FileKeys)
                        {
                            await _userMediaService.AddUserMedia(userId, key, MediaType.Image, UserMediaStatus.Confirmed);
                        }

                        return true;

                    case "story":
                    case "cover-story":
                        var story = await _storyService.GetStoryByIdAsync(request.ResourceId.Value);

                        if (story == null)
                            return false;

                        if (story.User.Id != userId)
                            return false;

                        if (request.ResourceType == "story")
                        {
                            var check = await CreateMediaStory(request.ResourceId.Value, request.FileKeys);
                            return check;
                        }
                        else
                        {
                            story.CoverImageUrl = request.FileKeys[0].ToLower().Trim();
                            await _storyService.UpdateWithEntityAsync(story);
                            return true;
                        }       
                }
                return false;
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to confirm upload: {ex.Message}");
            }
        }

        private async Task<bool> CreateMediaStory(int storyId, List<string> url)
        {
            // Remove existing media
            await _mediaService.DeleteMediaByStoryIdAsync(storyId);

            var media = await _mediaService.CreateMediaAsync(storyId, url);
            if (media < 0)
                return false;

            return true;
        }

        public async Task<UploadMediaResponse> UploadMedia(UploadMediaRequest request, long userId)
        {
            try
            {

                var uploads = new List<PresignUrlResponse>();
                var cdnDomain = Environment.GetEnvironmentVariable("CDN_DOMAIN");

                for (int i = 0; i < request.Files.Count; i++)
                {
                    var contentType = request.Files[i].ContentType.Trim();
                    var extension = contentType switch
                    {
                        "image/jpeg" => "jpg",
                        "image/png" => "png",
                        "image/gif" => "gif",
                        "image/webp" => "webp",
                        "image/bmp" => "bmp",
                        "image/svg+xml" => "svg",
                        "image/avif" => "avif",
                        _ => "img"
                    };

                    string guid = Guid.NewGuid().ToString("N");
                    string key = request.ResourceType switch
                    {
                        "avatar" => $"avatars/{userId}/{guid}.{extension}",
                        "cover-user" => $"user-uploads/{userId}/{guid}.{extension}",
                        "cover-story" => $"story-assets/cover/cover_{guid}.{extension}",
                        "story" => $"story-assets/content/story_{guid}.{extension}",
                        "comment" => $"comments/{userId}/{guid}.{extension}",
                        _ => $"others/{userId}/{guid}.{extension}"
                    };

                    // Media link
                    var url = $"{cdnDomain}/{key}";

                    // Generate presign url
                    var s3Url = _s3Service.GeneratePresignUrl(key, contentType);

                    uploads.Add(new PresignUrlResponse
                    {
                        S3Url = s3Url,
                        MediaUrl = url,
                        Key = key
                    });
                }

                return new UploadMediaResponse { Uploads = uploads };
            }
            catch (Exception ex)
            {
                throw new Exception($"Failed to generate presign URL: {ex.Message}");
            }
        }
    }
}
