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
        Task<List<Story>> GetStoriesPreviewAsync(int limit, DateTime? cursor);
    }
}
