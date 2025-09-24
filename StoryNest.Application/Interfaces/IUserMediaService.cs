
using StoryNest.Domain.Enums;

namespace StoryNest.Application.Interfaces
{
    public interface IUserMediaService
    {
        public Task<int> AddUserMedia(long userId, string key, MediaType type, UserMediaStatus status);
    }
}
