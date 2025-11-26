using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Interfaces
{
    public interface IStoryRepository
    {
        public Task AddAsync(Story story);
        public Task<List<Story>> GetStoriesByUserAsync(long userId, DateTime? cursor, bool isOwner, bool excludeAiMedia, bool onlyAiMedia);
        public Task<List<Story>> GetStoriesPreviewAsync(int limit, long cursor = 0);
        public Task<List<Story>> GetSmartRecommendedStoriesAsync(long userId, int limit, long cursor = 0);
        public Task<Story> GetStoryByIdOrSlugAsync(int? storyId, string? slug, bool asNoTracking = false);
        public Task<Story> GetStoryByIdOrSlugDeleteAsync(int? storyId, string? slug, bool asNoTracking = false);
        public Task<Story> GetStoryByIdOrSlugOwnerAsync(int? storyId, string? slug, bool asNoTracking = false);
        public Task<Story> GetStoryByIdOrSlugUpdateAsync(int? storyId, string? slug, bool asNoTracking = false);
        public void RemoveStory(Story story); 
        public void UpdateStory(Story story);
        public Task<List<Story>> SearchAsync(string keyword, int limit = 20, int? lastId = null);
        public Task<int> TotalStories();
        public Task<List<Story>> GetAllStories(int page = 1, int pageSize = 10);
    }
}
