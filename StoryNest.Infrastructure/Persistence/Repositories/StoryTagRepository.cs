using Microsoft.EntityFrameworkCore;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Persistence.Repositories
{
    public class StoryTagRepository : IStoryTagRepository
    {
        private readonly MyDbContext _context;

        public StoryTagRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(StoryTag storyTag)
        {
            await _context.StoryTags.AddAsync(storyTag);
        }

        public async Task<bool> GetStoryTagAsync(int storyId = 0, int tagId = 0)
        {
            var storyTag = await _context.StoryTags.FirstOrDefaultAsync(st => st.StoryId == storyId && st.TagId == tagId);

            if (storyTag != null)
                return true;

            return false;
        }

        public async Task RemoveAsync(int storyId, int tagId)
        {
            await _context.StoryTags
                .Where(st => st.StoryId == storyId && st.TagId == tagId)
                .ExecuteDeleteAsync();
        }
    }
}
