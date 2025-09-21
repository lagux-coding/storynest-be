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
    public class LikeRepository : ILikeRepository
    {
        private readonly MyDbContext _context;

        public LikeRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddLikeAsync(Like like)
        {
            await _context.Likes.AddAsync(like);
        }

        public async Task<int> CountLikeAsync(int storyId)
        {
            return await _context.Likes
                .CountAsync(l => l.StoryId == storyId && l.RevokedAt == null);
        }

        public async Task<List<User>> GetAllUserLikeAsync(int storyId)
        {
            var users = await _context.Likes
                .Where(l => l.StoryId == storyId && l.RevokedAt == null)
                .Select(l => l.User)
                .ToListAsync();

            return users;
        }

        public async Task<Like?> GetLikeAsync(int? storyId, long? userId)
        {
            return await _context.Likes
                .FirstOrDefaultAsync(l => l.StoryId == storyId || l.UserId == userId);
        }

        public async Task<User?> GetUserLikeAsync(int storyId, long userId)
        {
            var user = await _context.Likes
                .Where(l => l.StoryId == storyId && l.UserId == userId && l.RevokedAt == null)
                .Select(l => l.User)
                .FirstOrDefaultAsync();

            return user;
        }

        public async Task RemoveLikeAsync(int storyId, long userId)
        {
            var like = await _context.Likes
                        .FirstOrDefaultAsync(l => l.StoryId == storyId && l.UserId == userId && l.RevokedAt == null);

            if (like != null)
            {
                like.RevokedAt = DateTime.UtcNow;
                _context.Likes.Update(like);
            }
            else
            {
                throw new Exception("Like not found");
            }
        }

        public async Task UpdateAsync(Like like)
        {
            _context.Likes.Update(like);
        }
    }
}
