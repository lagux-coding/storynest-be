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
    public class MediaService : IMediaService
    {
        private readonly IMediaRepository _mediaRepository;
        private readonly IUnitOfWork _unitOfWork;

        public MediaService(IMediaRepository mediaRepository, IUnitOfWork unitOfWork)
        {
            _mediaRepository = mediaRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> CreateAudioMediaAsync(int storyId, string url)
        {
            var media = new Media
            {
                StoryId = storyId,
                MediaUrl = url,
                MediaType = MediaType.Audio,
                Caption = "story audio",
                MimeType = "audio",
                CreatedAt = DateTime.UtcNow
            };
            await _mediaRepository.CreateMediaAsync(media);
            return await _unitOfWork.SaveAsync();
        }

        public async Task<int> CreateMediaAsync(int storyId, List<string> url)
        {
            foreach (var imgUrl in url)
            {
                var media = new Media
                {
                    StoryId = storyId,
                    MediaUrl = imgUrl,
                    MediaType = MediaType.Image,
                    Caption = "story image",
                    MimeType = "image",
                    CreatedAt = DateTime.UtcNow
                };
                await _mediaRepository.CreateMediaAsync(media);
            }

            return await _unitOfWork.SaveAsync();
        }

        public async Task<int> DeleteMediaByStoryIdAsync(int storyId)
        {
            await _mediaRepository.DeleteMediaByStoryId(storyId);
            return await _unitOfWork.SaveAsync();
        }
    }
}
