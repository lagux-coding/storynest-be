using Microsoft.EntityFrameworkCore;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Persistence.Repositories
{
    public class StoryRepository : IStoryRepository
    {
        private readonly MyDbContext _context;

        public StoryRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Story story)
        {
            await _context.Stories.AddAsync(story);
        }

        public Task<List<Story>> GetStoriesByUserAsync(long userId, DateTime? cursor, bool isOwner)
        {
            var query = _context.Stories.AsQueryable();
            if (!isOwner)
            {
                query = query.Where(s => s.StoryStatus == StoryStatus.Published && s.PrivacyStatus == PrivacyStatus.Public);
            }
            if (cursor.HasValue)
            {
                query = query.Where(s => s.CreatedAt < cursor.Value);
            }

            return query
                    .Where(s => s.UserId == userId)
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(10 + 1)
                    .Include(s => s.User)
                    .Include(m => m.Media)
                    .Include(st => st.StoryTags)
                        .ThenInclude(t => t.Tag)
                    .Include(l => l.Likes)
                    .Include(c => c.Comments)
                    .ToListAsync();
        }

        public async Task<List<Story>> GetStoriesPreviewAsync(int limit, DateTime? cursor)
        {
            var query = _context.Stories.AsQueryable();

            if (cursor.HasValue)
            {
                query = query.Where(s => s.CreatedAt < cursor.Value);
            }

            return await query
                    .Where(s => s.PrivacyStatus == Domain.Enums.PrivacyStatus.Public && 
                            s.StoryStatus == Domain.Enums.StoryStatus.Published &&
                            s.User.IsActive == true)
                    .OrderByDescending(s => s.CreatedAt)
                    .Take(limit + 1)
                    .Include(s => s.User)
                    .Include(m => m.Media)
                    .Include(st => st.StoryTags)
                        .ThenInclude(t => t.Tag)
                    .Include(l => l.Likes)
                    .Include(c => c.Comments)
                    .ToListAsync();
        }

        public async Task<Story> GetStoryByIdOrSlugAsync(int? storyId, string? slug)
        {
            var query = _context.Stories.AsQueryable();
            query = query
                .Include(s => s.User)
                .Include(m => m.Media)
                .Include(st => st.StoryTags)
                    .ThenInclude(t => t.Tag)
                .Include(l => l.Likes)
                .Include(c => c.Comments);

            if (storyId.HasValue)
            {
                return await query.FirstOrDefaultAsync(s => s.Id == storyId.Value && s.User.IsActive == true);
            }
            else if (!string.IsNullOrEmpty(slug))
            {
                string normalizedSlug = slug.ToLower();
                return await query.FirstOrDefaultAsync(s => s.Slug.ToLower() == normalizedSlug && s.User.IsActive == true);
            }

            return null;
        }

        public void RemoveStory(Story story)
        {
            _context.Stories.Remove(story);
        }

        public void UpdateStory(Story story)
        {
            _context.Stories.Update(story);
        }
    }
}
