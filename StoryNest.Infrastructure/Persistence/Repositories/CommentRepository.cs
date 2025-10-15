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
    public class CommentRepository : ICommentRepository
    {
        private readonly MyDbContext _context;

        public CommentRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Comment comment)
        {
            await _context.Comments.AddAsync(comment);
        }

        public async Task<int> CountComments(int storyId)
        {
            return await _context.Comments
                .Where(c => c.StoryId == storyId && c.DeletedAt == null)
                .CountAsync();
        }

        public async Task<Comment?> GetByIdAsync(int commentId)
        {
            return await _context.Comments
                .Include(c => c.User)
                .FirstOrDefaultAsync(c => c.Id == commentId && c.DeletedAt == null);
        }

        public async Task<List<Comment>> GetByStoryId(int storyId, int? parentId, int limit, int? cursor)
        {
            var query = _context.Comments
                .Where(c => c.StoryId == storyId && c.ParentCommentId == parentId);

            if (cursor.HasValue && cursor.Value > 0)
                query = query.Where(c => c.Id < cursor.Value);

            return await query
                .OrderByDescending(c => c.CreatedAt)
                .Take(limit + 1) // +1 để check hasMore
                .Include(c => c.User)
                .ToListAsync();
        }

        public async Task<List<Comment>> GetByUserId(long userId, int limit, int cursor = 0)
        {
            var query = _context.Comments
                .Where(c => c.UserId == userId && c.DeletedAt == null && c.CommentStatus != CommentStatus.Deleted)
                .OrderByDescending(c => c.CreatedAt)
                .AsQueryable();

            if (cursor > 0)
                query = query.Where(c => c.Id < cursor);

            var comments = await query
                .Include(c => c.Story)
                .Take(limit + 1)
                .ToListAsync();

            return comments;
        }

        public async Task<List<int>> GetCommentIdsWithReplies(List<int> ids)
        {
            return await _context.Comments
                .Where(c => c.ParentCommentId != null
                            && ids.Contains(c.ParentCommentId.Value)
                            && c.DeletedAt == null)
                .Select(c => c.ParentCommentId.Value)
                .Distinct()
                .ToListAsync();
        }

        public async Task<Dictionary<int, int>> GetRepliesCount(List<int> parentId)
        {
            return await _context.Comments
                .Where(c => c.ParentCommentId != null
                        && parentId.Contains(c.ParentCommentId.Value)
                        && c.DeletedAt == null)
                .GroupBy(c => c.ParentCommentId.Value)
                .Select(g => new { ParentId = g.Key, Count = g.Count() })
                .ToDictionaryAsync(x => x.ParentId, x => x.Count);
        }

        public async Task<Story?> GetStoryByCommentAsync(int commentId)
        {
            return await _context.Stories
                .Include(s => s.Comments)
                .FirstOrDefaultAsync(s => s.Comments.Where(c => c.CommentStatus != CommentStatus.Deleted).Any(c => c.Id == commentId) && s.StoryStatus == StoryStatus.Published && s.PrivacyStatus == PrivacyStatus.Public);
        }

        public Task UpdateAsync(Comment comment)
        {
            _context.Comments.Update(comment);
            return Task.CompletedTask;
        }
    }
}
