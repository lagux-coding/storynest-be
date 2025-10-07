using StoryNest.Application.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface ILikeService
    {
        Task<List<UserBasicResponse>> GetAllUserLikesAsync(int storyId);
        Task<UserBasicResponse?> GetUserLikeAsync(int storyId, long userId);
        Task<int> LikeStoryAsync(int storyId, long userId);
        Task<int> UnlikeStoryAsync(int storyId, long userId);
    }
}
