using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Interfaces
{
    public interface IStoryTagRepository
    {
        public Task AddAsync(StoryTag storyTag);
        public Task<bool> GetStoryTagAsync(int storyId = 0, int tagId = 0);
    }
}
