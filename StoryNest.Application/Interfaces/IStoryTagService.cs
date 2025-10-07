using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IStoryTagService
    {
        public Task AddStoryTagAsync(StoryTag storyTag);
        public Task RemoveStoryTagAsync(int storyId, int tagId);
        public Task<bool> GetStoryTagAsync(int storyId = 0, int tagId = 0);
    }
}
