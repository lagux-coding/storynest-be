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

        public async Task<List<Story>> GetRecommendedStoriesAsync(long? userId, int limit, DateTime? cursor)
        {
            // ========== 🧱 BASE QUERY ==========
            IQueryable<Story> baseQuery = _context.Stories
                .AsNoTracking()
                .Where(s => s.PrivacyStatus == PrivacyStatus.Public &&
                            s.StoryStatus == StoryStatus.Published &&
                            s.User.IsActive);

            // Optional cursor buffer để tránh miss story có timestamp trùng
            if (cursor.HasValue)
            {
                var cursorBuffer = cursor.Value.AddMilliseconds(-1);
                baseQuery = baseQuery.Where(s => s.CreatedAt < cursorBuffer);
            }

            // ========== 🟣 1️⃣ USER CHƯA LOGIN ==========
            if (userId == null)
            {
                var feed = baseQuery
                    .OrderByDescending(s => s.CreatedAt)
                    .ThenByDescending(s => s.Id)
                    .Take(limit + 1);

                var ids = await feed.Select(s => s.Id).ToListAsync();
                return await LoadFullStoriesPreservingOrder(ids);
            }

            // ========== 🟣 2️⃣ XÁC ĐỊNH GU USER ==========
            var preferredTagIds = await _context.Likes
                .AsNoTracking()
                .Where(l => l.UserId == userId && l.RevokedAt == null)
                .OrderByDescending(l => l.CreatedAt)
                .Take(500)
                .SelectMany(l => l.Story.StoryTags.Select(st => st.TagId))
                .GroupBy(id => id)
                .OrderByDescending(g => g.Count())
                .Take(5)
                .Select(g => g.Key)
                .ToListAsync();

            // ========== 🟣 3️⃣ LẤY CANDIDATES (RECOMMEND + HOT) ==========
            IQueryable<Story> recommendQuery = baseQuery
                .Where(s => s.UserId != userId);

            if (preferredTagIds.Any())
            {
                recommendQuery = recommendQuery
                    .Where(s => s.StoryTags.Any(st => preferredTagIds.Contains(st.TagId)));
            }

            // Lấy dư 3x limit để re-rank
            var recommendRaw = await recommendQuery
                .Select(s => new
                {
                    s.Id,
                    s.CreatedAt,
                    s.LikeCount,
                    MatchCount = preferredTagIds.Count == 0
                        ? 0
                        : s.StoryTags.Count(st => preferredTagIds.Contains(st.TagId))
                })
                .Take(limit * 3)
                .ToListAsync();

            // ========== 🧮 4️⃣ RANK SCORE ==========
            var rankedIds = recommendRaw
                .Select(x =>
                {
                    double daysOld = (DateTime.UtcNow - x.CreatedAt).TotalDays;
                    double recency = 1.0 / (1.0 + Math.Max(0.0, daysOld));
                    double score = (x.MatchCount * 0.6) + (x.LikeCount * 0.3) + (recency * 0.1);
                    return new { x.Id, x.CreatedAt, Score = score };
                })
                .OrderByDescending(z => z.Score)
                .ThenByDescending(z => z.CreatedAt)
                .Select(z => z.Id)
                .ToList();

            // ========== 🟣 5️⃣ FALLBACK NẾU CHƯA ĐỦ ==========
            if (rankedIds.Count < limit)
            {
                var remaining = limit - rankedIds.Count;

                IQueryable<Story> fallbackQuery = baseQuery
                    .Where(s => s.UserId != userId && !rankedIds.Contains(s.Id))
                    .OrderByDescending(s => s.LikeCount)
                    .ThenByDescending(s => s.CreatedAt);

                var fallbackIds = await fallbackQuery
                    .Take(remaining * 2)
                    .Select(s => s.Id)
                    .ToListAsync();

                rankedIds.AddRange(fallbackIds);
            }

            // Loại trùng ID nếu story xuất hiện cả ở recommend + fallback
            rankedIds = rankedIds.Distinct().ToList();

            // ========== 🧮 6️⃣ APPLY CURSOR LẦN CUỐI (GIỮ FE NGUYÊN) ==========
            if (cursor.HasValue)
                rankedIds = await _context.Stories
                    .AsNoTracking()
                    .Where(s => rankedIds.Contains(s.Id) && s.CreatedAt < cursor.Value)
                    .OrderByDescending(s => s.CreatedAt)
                    .ThenByDescending(s => s.Id)
                    .Select(s => s.Id)
                    .ToListAsync();

            // ========== 🧩 7️⃣ LẤY STORY THEO THỨ TỰ & GIỚI HẠN ==========
            rankedIds = rankedIds
                .OrderByDescending(id => recommendRaw.FirstOrDefault(r => r.Id == id)?.CreatedAt ?? DateTime.MinValue)
                .Take(limit + 1)
                .ToList();

            // ========== 🧩 8️⃣ LOAD FULL DATA SAU ==========
            return await LoadFullStoriesPreservingOrder(rankedIds);

            // ========== 🔧 HELPER: LOAD FULL STORY ==========
            async Task<List<Story>> LoadFullStoriesPreservingOrder(List<int> ids)
            {
                if (ids == null || ids.Count == 0) return new List<Story>();

                var indexMap = ids.Select((id, idx) => new { id, idx })
                                  .ToDictionary(x => x.id, x => x.idx);

                var fullStories = await _context.Stories
                    .AsNoTracking()
                    .Where(s => ids.Contains(s.Id))
                    .Include(s => s.User)
                    .Include(s => s.Media)
                    .Include(s => s.StoryTags).ThenInclude(st => st.Tag)
                    .Include(s => s.Likes)
                    .Include(s => s.Comments)
                    .ToListAsync();

                // giữ thứ tự đúng
                return fullStories.OrderBy(s => indexMap[s.Id]).ToList();
            }
        }


    }
}
