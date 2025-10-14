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

        public async Task<List<Story>> GetStoriesPreviewAsync(int limit, long cursor = 0)
        {
            var query = _context.Stories
                .Where(s => s.PrivacyStatus == Domain.Enums.PrivacyStatus.Public &&
                            s.StoryStatus == Domain.Enums.StoryStatus.Published &&
                            s.User.IsActive)
                .Include(s => s.User)
                .Include(s => s.Media)
                .Include(s => s.StoryTags)
                    .ThenInclude(st => st.Tag)
                .Include(s => s.Likes)
                .Include(s => s.Comments)
                .OrderByDescending(s => s.Id) // sắp theo Id mới nhất
                .AsQueryable();

            if (cursor > 0)
            {
                // chỉ lấy story có Id nhỏ hơn cursor (tức là sau story cuối của trang trước)
                query = query.Where(s => s.Id < cursor);
            }

            return await query
                .Take(limit + 1) // lấy thêm 1 để xác định HasMore
                .ToListAsync();
        }


        public async Task<List<Story>> GetSmartRecommendedStoriesAsync(long userId, int limit, long offset = 0) // đổi cursor → offset
        {
            // 1️⃣ Lấy user tags
            var userTagIds = await _context.Stories
                .Where(s =>
                    s.Likes.Any(l => l.UserId == userId && l.RevokedAt == null) ||
                    s.Comments.Any(c => c.UserId == userId) ||
                    s.StoryViews.Any(v => v.UserId == userId))
                .SelectMany(s => s.StoryTags.Select(st => st.TagId))
                .Distinct()
                .ToListAsync();

            // 2️⃣ Lấy pool lớn hơn để score
            var candidateSize = Math.Max(200, (offset + limit) * 3);
            var candidates = await _context.Stories
                .Where(s => s.PrivacyStatus == PrivacyStatus.Public &&
                            s.StoryStatus == StoryStatus.Published &&
                            s.User.IsActive)
                .OrderByDescending(s => s.CreatedAt) // thứ tự cố định
                .Take((int)candidateSize)
                .Include(s => s.User)
                .Include(s => s.Media)
                .Include(s => s.StoryTags).ThenInclude(st => st.Tag)
                .Include(s => s.Likes)
                .Include(s => s.Comments)
                .Include(s => s.StoryViews)
                .ToListAsync();

            // 3️⃣ Score + rank
            var ranked = candidates
                .Select(s => new
                {
                    Story = s,
                    Score = CalculateScore(s, userTagIds, userId)
                })
                .OrderByDescending(x => x.Score)
                .ThenByDescending(x => x.Story.CreatedAt) // tie-breaker
                .Skip((int)offset)
                .Take(limit + 1)
                .Select(x => x.Story)
                .ToList();

            return ranked;
        }

        private double CalculateScore(Story s, List<int> userTagIds, long userId)
        {
            var tagMatch = s.StoryTags.Count(st => userTagIds.Contains(st.TagId)) * 5.0;
            var engagement = s.Likes.Count * 0.3 + s.Comments.Count * 0.2 + s.StoryViews.Count * 0.1;
            var recency = Math.Max(0, 10 - (DateTime.UtcNow - s.CreatedAt).TotalDays);
            var notViewed = !s.StoryViews.Any(v => v.UserId == userId) ? 2.0 : 0;

            return tagMatch + engagement + recency + notViewed;
        }


        public async Task<Story> GetStoryByIdOrSlugAsync(int? storyId, string? slug, bool asNoTracking = false)
        {
            try
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
                    return await query.FirstOrDefaultAsync(s => s.Id == storyId.Value && s.PrivacyStatus == PrivacyStatus.Public && s.StoryStatus == StoryStatus.Published && s.User.IsActive == true);
                }
                else if (!string.IsNullOrEmpty(slug))
                {
                    string normalizedSlug = slug.ToLower();
                    return await query.FirstOrDefaultAsync(s => s.Slug.ToLower() == normalizedSlug && s.User.IsActive == true);
                }

                return null;
            }
            catch (Exception ex)
            {
                var msg = ex.Message;
                throw;
            }
        }

        public async Task<Story> GetStoryByIdOrSlugUpdateAsync(int? storyId, string? slug, bool asNoTracking = false)
        {
            try
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
            catch (Exception ex)
            {
                var msg = ex.Message;
                throw;
            }
        }

        public async Task<Story> GetStoryByIdOrSlugOwnerAsync(int? storyId, string? slug, bool asNoTracking = false)
        {
            try
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
            catch
            {
                throw;
            }
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
