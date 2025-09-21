using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Interfaces
{
    public interface ILikeRepository
    {
        Task<User?> GetUserLikeAsync(int storyId, long userId);
        Task<List<User>> GetAllUserLikeAsync(int storyId);
        Task AddLikeAsync(Like like);
        Task RemoveLikeAsync(int storyId, long userId);
    }
}
