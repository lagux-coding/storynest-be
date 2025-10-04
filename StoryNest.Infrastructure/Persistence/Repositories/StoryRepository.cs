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

        public Task<List<Story>> GetStoriesByUserAsync(long userId, DateTime? cursor, bool isOwner, bool excludeAiMedia = false, bool onlyAiMedia = false)
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
            if (excludeAiMedia)
            {
                query = query.Where(s =>
                    !s.Media.Any(m => EF.Functions.Like(m.MediaUrl, "%generated-content/%"))
                );
            }
            if (onlyAiMedia)
            {
                query = query.Where(s =>
                    s.Media.Any(m => EF.Functions.Like(m.MediaUrl, "%generated-content/%"))
                );
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

        public async Task<Story> GetStoryByIdOrSlugAsync(int? storyId, string? slug, bool asNoTracking = false)
        {
            IQueryable<Story> query = _context.Stories;

            if (asNoTracking)
                query = query.AsNoTracking();

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

        public async Task<List<Story>> SearchAsync(string? keyword, int limit = 20, int? lastId = null)
        {
            // Nếu keyword null hoặc rỗng => trả về danh sách story bình thường
            if (string.IsNullOrWhiteSpace(keyword))
            {
                var query = _context.Stories
                    .Where(s => s.PrivacyStatus == PrivacyStatus.Public &&
                                s.StoryStatus == StoryStatus.Published &&
                                s.User.IsActive);

                if (lastId.HasValue)
                {
                    query = query.Where(s => s.Id < lastId);
                }

                return await query
                    .OrderByDescending(s => s.Id)
                    .Take(limit)
                    .Include(s => s.User)
                    .Include(s => s.Media)
                    .Include(st => st.StoryTags).ThenInclude(t => t.Tag)
                    .Include(l => l.Likes)
                    .Include(c => c.Comments)
                    .ToListAsync();
            }

            // Nếu có keyword thì chạy FTS + Tag như cũ
            List<int> ids;

            if (lastId.HasValue)
            {
                ids = await _context.Stories
                    .FromSqlInterpolated($@"
                        WITH q AS (SELECT plainto_tsquery('simple', unaccent({keyword})) AS query)
                        SELECT DISTINCT s.id
                        FROM ""Stories"" s
                        LEFT JOIN ""StoryTags"" st ON s.id = st.story_id
                        LEFT JOIN ""Tags"" t ON t.id = st.tag_id, q
                        WHERE (
                                s.""SearchVector"" @@ q.query
                                OR unaccent(t.name) ILIKE '%' || unaccent({keyword}) || '%'
                              )
                          AND s.privacy_status = 'Public'
                          AND s.story_status = 'Published'
                          AND s.id < {lastId}
                        ORDER BY s.id DESC
                        LIMIT {limit}
                    ")
                    .Select(s => s.Id)
                    .ToListAsync();
            }
            else
            {
                ids = await _context.Stories
                    .FromSqlInterpolated($@"
                        WITH q AS (SELECT plainto_tsquery('simple', unaccent({keyword})) AS query)
                        SELECT DISTINCT s.id
                        FROM ""Stories"" s
                        LEFT JOIN ""StoryTags"" st ON s.id = st.story_id
                        LEFT JOIN ""Tags"" t ON t.id = st.tag_id, q
                        WHERE (
                                s.""SearchVector"" @@ q.query
                                OR unaccent(t.name) ILIKE '%' || unaccent({keyword}) || '%'
                              )
                          AND s.privacy_status = 'Public'
                          AND s.story_status = 'Published'
                        ORDER BY s.id DESC
                        LIMIT {limit}
                    ")
                    .Select(s => s.Id)
                    .ToListAsync();
            }

            if (!ids.Any())
                return new List<Story>();

            return await _context.Stories
                .Where(s => ids.Contains(s.Id) && s.User.IsActive)
                .Include(s => s.User)
                .Include(s => s.Media)
                .Include(st => st.StoryTags).ThenInclude(t => t.Tag)
                .Include(l => l.Likes)
                .Include(c => c.Comments)
                .OrderByDescending(s => s.Id)
                .ToListAsync();
        }



        public void UpdateStory(Story story)
        {
            _context.Stories.Update(story);
        }
    }
}
