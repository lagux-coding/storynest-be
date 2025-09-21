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

        public async Task<List<User>> GetAllUserLikeAsync(int storyId)
        {
            var users = _context.Likes
                .Where(l => l.StoryId == storyId && l.RevokedAt == null)
                .Select(l => l.User)
                .ToList();

            return users;
        }

        public async Task<User?> GetUserLikeAsync(int storyId, long userId)
        {
            var user = _context.Likes
                .Where(l => l.StoryId == storyId && l.UserId == userId && l.RevokedAt == null)
                .Select(l => l.User)
                .FirstOrDefault();

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
        }
    }
}
