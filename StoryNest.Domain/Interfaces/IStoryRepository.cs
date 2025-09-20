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
        public Task<List<Story>> GetStoriesPreviewAsync(int limit, DateTime? cursor);
        public Task<Story> GetStoryByIdOrSlugAsync(int? storyId, string? slug);
        public void RemoveStory(Story story); 
        public void UpdateStory(Story story);
    }
}
