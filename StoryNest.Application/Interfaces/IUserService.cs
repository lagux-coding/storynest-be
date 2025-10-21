using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(long userId);
        Task<UserFullResponse> GetMe(long userId);
        Task<PaginatedResponse<CommentResponse>> GetUserCommentAsync(long userId, int limit, int cursor = 0);
        Task<StoryResponse> GetStoryByCommentAsync(int commentId);
        Task<List<User>> GetAllUser();
        Task UpdateUserAsync(User user);
        Task<int> UpdateUserProfille(long userId, UpdateUserProfileRequest request);
        Task<int> ChangePasswordAsync(ChangePasswordRequest request, long userId);
    }
}
