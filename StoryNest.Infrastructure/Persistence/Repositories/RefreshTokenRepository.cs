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
    public class RefreshTokenRepository : IRefreshTokenRepository
    {
        private readonly MyDbContext _context;

        public RefreshTokenRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(RefreshTokens token)
        {
            await _context.RefreshTokens.AddAsync(token);
        }

        public async Task<RefreshTokens?> GetByHashAsync(string tokenHash)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
        }

        public async Task UpdateAsync(RefreshTokens token)
        {
            _context.RefreshTokens.Update(token);
            await Task.CompletedTask;
        }
    }
}
