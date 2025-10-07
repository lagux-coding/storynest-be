using AutoMapper;
using Microsoft.AspNetCore.Mvc;
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
    public class LikeService : ILikeService
    {
        private readonly ILikeRepository _likeRepository;
        private readonly IStoryService _storyService;
        private readonly INotificationService _notificationService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public LikeService(ILikeRepository likeRepository, IMapper mapper, IUnitOfWork unitOfWork, IStoryService storyService, INotificationService notificationService)
        {
            _likeRepository = likeRepository;
            _mapper = mapper;
            _unitOfWork = unitOfWork;
            _storyService = storyService;
            _notificationService = notificationService;
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
                var like = await _likeRepository.GetLikeAsync(storyId, userId);
                if (like == null)
                {
                    like = new Like
                    {
                        StoryId = storyId,
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _likeRepository.AddLikeAsync(like);
                }
                else if (like.RevokedAt != null)
                {
                    like.RevokedAt = null;
                    await _likeRepository.UpdateAsync(like);
                }
                else
                {
                    return 0; // User has already liked the story
                }
                await _unitOfWork.SaveAsync();

                var likeCount = await _likeRepository.CountLikeAsync(storyId);
                var story = await _storyService.GetStoryByIdAsync(storyId);
                if (story == null)
                {
                    return 0; // Story not found
                }
                story.LikeCount = likeCount;
               
                var result = await _storyService.UpdateWithEntityAsync(story); // Include unit of work save

                var content = $"{story.User.Username} vừa yêu thích câu chuyện <strong>{story.Title}</strong> của bạn.";
                await _notificationService.SendNotificationAsync(
                    story.UserId,
                    userId,
                    content,
                    NotificationType.StoryLiked,
                    storyId,
                    "story"
                );

                return result;
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message);
            }
        }

        public async Task<int> UnlikeStoryAsync(int storyId, long userId)
        {
            try
            {
                var like = await _likeRepository.GetLikeAsync(storyId, userId);
                if (like == null || like.RevokedAt != null)
                {
                    return 0;
                }

                like.RevokedAt = DateTime.UtcNow;
                await _likeRepository.UpdateAsync(like);
                await _unitOfWork.SaveAsync();

                var likeCount = await _likeRepository.CountLikeAsync(storyId);
                var story = await _storyService.GetStoryByIdAsync(storyId);
                if (story == null)
                {
                    return 0; // Story not found
                }

                story.LikeCount = likeCount;
                return await _storyService.UpdateWithEntityAsync(story); // Include unit of work save
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
