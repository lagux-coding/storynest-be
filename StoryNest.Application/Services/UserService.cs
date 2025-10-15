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
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICommentService _commentService;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper, ICommentService commentService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _commentService = commentService;
        }

        public async Task<List<User>> GetAllUser()
        {
            return await _userRepository.GetAllUserAsync();
        }

        public async Task<UserFullResponse> GetMe(long userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) throw new Exception("User not found or not active anymore");
                var userDto = _mapper.Map<UserFullResponse>(user);
                return userDto;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<StoryResponse> GetStoryByCommentAsync(int commentId)
        {
            try
            {
                var story = await _commentService.GetStoryByCommentAsync(commentId);
                if (story == null) return null;
                var storyDto = _mapper.Map<StoryResponse>(story);
                return storyDto;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return null;
            return user;
        }

        public Task<User?> GetUserByIdAsync(long userId)
        {
            var user = _userRepository.GetByIdAsync(userId);
            if (user == null) return null;
            return user;
        }

        public async Task<PaginatedResponse<CommentResponse>> GetUserCommentAsync(long userId, int limit, int cursor = 0)
        {
            try
            {
                var comments = await _commentService.GetCommentByUserAsync(userId, limit, cursor);
                if (comments.Count == 0)
                    return new PaginatedResponse<CommentResponse>()
                    {
                        Items = new List<CommentResponse>(),
                        HasMore = false,
                        NextCursor = null
                    };
                var hasMore = comments.Count > limit;

                if (hasMore)
                    comments = comments.Take(limit).ToList();
                var response = _mapper.Map<List<CommentResponse>>(comments);
                foreach (var c in response)
                {
                    c.IsOwner = true;
                }

                var nextCursor = response.LastOrDefault()?.Id.ToString();
                return new PaginatedResponse<CommentResponse>()
                {
                    NextCursor = hasMore ? nextCursor : null,
                    Items = response,
                    HasMore = hasMore
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdateUserAsync(User user)
        {
            await _userRepository.UpdateAsync(user);
        }
    }
}
