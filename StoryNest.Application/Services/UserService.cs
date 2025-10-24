using AutoMapper;
using Microsoft.Extensions.Configuration;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Features.Users;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Interfaces;
using StoryNest.Shared.Common.Utils;
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
        private readonly IAICreditService _aiCreditService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IMapper _mapper;
        private readonly ChangePasswordSuccessEmailSender _email;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _config;

        public UserService(IUserRepository userRepository, IMapper mapper, ICommentService commentService, ICurrentUserService currentUserService, IUnitOfWork unitOfWork, ChangePasswordSuccessEmailSender email, IConfiguration config, IAICreditService aiCreditService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _commentService = commentService;
            _currentUserService = currentUserService;
            _unitOfWork = unitOfWork;
            _email = email;
            _config = config;
            _aiCreditService = aiCreditService;
        }

        public async Task<int> ChangePasswordAsync(ChangePasswordRequest request, long userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                    throw new Exception("User not found or not active anymore");

                // Verify new password and confirm password
                if (request.NewPassword != request.ConfirmPassword)
                {
                    throw new Exception("New password and confirm password do not match");
                }

                // Verify old password
                if (PasswordHelper.VerifyPassword(request.OldPassword, user.PasswordHash))
                {
                    // Update to new password
                    user.PasswordHash = PasswordHelper.HashPassword(request.NewPassword);
                    user.UpdatedAt = DateTime.UtcNow;
                    await _userRepository.UpdateAsync(user);

                    // Send email
                    await _email.SendAsync(user.Email, user.Username, user.Username, $"{_config["FRONTEND_URL"]}/forgot-password", CancellationToken.None);

                    return await _unitOfWork.SaveAsync();
                }
                else
                {
                    throw new Exception("Old password is incorrect");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
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

                var credits = await _aiCreditService.GetUserCredit(userId);

                var userDto = _mapper.Map<UserFullResponse>(user);
                userDto.Credits = credits.TotalCredits;
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
                var userId = _currentUserService.UserId;
                var story = await _commentService.GetStoryByCommentAsync(commentId, userId.Value);
                if (story == null) return null;
                var storyDto = _mapper.Map<StoryResponse>(story);              

                var isLike = story.Likes.FirstOrDefault(s => s.UserId == userId);
                if (isLike != null)
                {
                    storyDto.IsLiked = true;
                }
                else
                {
                    storyDto.IsLiked = false;
                }

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

        public async Task<int> UpdateUserProfille(long userId, UpdateUserProfileRequest request)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) throw new Exception("User not found or not active anymore");

                // check username
                bool check = await _userRepository.CheckUsernameExist(request.UserName);
                if (check)
                {
                    throw new Exception("Already this username");
                }

                user = _mapper.Map(request, user);
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                return await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
