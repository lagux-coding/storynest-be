
using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;

namespace StoryNest.Application.Interfaces
{
    public interface IUserMediaService
    {
        public Task<int> AddUserMedia(long userId, string key, MediaType type, UserMediaStatus status);
        public Task<int> UpdateUserMedia(UserMedia media);
        public Task<List<UserMedia>> GetByUserAndUrls(long userId, List<string> urls);
    }
}
