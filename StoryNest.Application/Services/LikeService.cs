using AutoMapper;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Services
{
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IStoryService _storyService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LikeService(ILikeRepository likeRepository, IMapper mapper, IUnitOfWork unitOfWork, IStoryService storyService)
        {
            _likeRepository = likeRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _storyService = storyService;
        }

        public async Task<List<UserBasicResponse>> GetAllUserLikesAsync(int storyId)
        {
            try
            {
                var users = await _likeRepository.GetAllUserLikeAsync(storyId);
                if (users == null || users.Count == 0) return new List<UserBasicResponse>();
                var response = _mapper.Map<List<UserBasicResponse>>(users);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<UserBasicResponse?> GetUserLikeAsync(int storyId, long userId)
        {
            try
            {
                var user = await _likeRepository.GetUserLikeAsync(storyId, userId);
                if (user == null) return null;
                var response = _mapper.Map<UserBasicResponse?>(user);
                return response;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> LikeStoryAsync(int storyId, long userId)
        {
            try
            {
                // Check if the user has already liked the story
                var existingLike = await _likeRepository.GetUserLikeAsync(storyId, userId);
                if (existingLike != null)
                {
                    // User has already liked the story, no action needed
                    return 0; // Indicate that no new like was added
                }

                var like = new Like
                {
                    StoryId = storyId,
                    UserId = userId,
                    CreatedAt = DateTime.UtcNow
                };

                // Add count to story's like count
                var story = await _storyService.GetStoryByIdAsync(storyId);
                if (story == null)
                {
                    return 0; // Story not found
                }
                story.LikeCount += 1;

                await _likeRepository.AddLikeAsync(like);
                return await _storyService.UpdateWithEntityAsync(story); // Include unit of work save
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }

        public async Task<int> UnlikeStoryAsync(int storyId, long userId)
        {
            try
            {
                _likeRepository.RemoveLikeAsync(storyId, userId);
                return await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
