using Microsoft.EntityFrameworkCore;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Persistence.Repositories
{
    public class UserMediaRepository : IUserMediaRepository
    {
        private readonly MyDbContext _context;

        public UserMediaRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(UserMedia media)
        {
            await _context.UserMedias.AddAsync(media);  
        }

        public Task<bool> ExistsAsync(Expression<Func<UserMedia, bool>> predicate)
        {
            return _context.UserMedias.AnyAsync(predicate);
        }

        public async Task<List<UserMedia>> GetByUserAndUrls(long userId, List<string> urls)
        {
            if (urls == null || urls.Count == 0)
                return new List<UserMedia>();

            return await _context.UserMedias
                    .Where(m => m.UserId == userId && urls.Contains(m.MediaUrl))
                    .ToListAsync();
        }

        public async Task<List<UserMedia>> GetUserMediasAsync(long userId, MediaType? type = null)
        {
            IQueryable<UserMedia> query = _context.UserMedias.Where(u => u.UserId == userId && (u.Status == UserMediaStatus.Orphaned || u.Status == UserMediaStatus.Confirmed));

            if (type.HasValue)
            {
                query = query.Where(u => u.MediaType == type.Value);
            }

            return await query.ToListAsync();
        }

        public async Task UpdateAsync(UserMedia media)
        {
            _context.UserMedias.Update(media);
            await Task.CompletedTask;
        }
    }
}
