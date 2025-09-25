using Microsoft.EntityFrameworkCore;
using StoryNest.Domain.Entities;
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

        public async Task UpdateAsync(UserMedia media)
        {
            _context.UserMedias.Update(media);
            await Task.CompletedTask;
        }
    }
}
