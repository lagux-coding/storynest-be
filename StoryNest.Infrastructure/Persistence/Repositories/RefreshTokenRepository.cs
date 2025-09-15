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

        public async Task AddAsync(RefreshToken token)
        {
            await _context.RefreshTokens.AddAsync(token);
        }

        public async Task<RefreshToken?> GetByHashAsync(string tokenHash)
        {
            return await _context.RefreshTokens
                .FirstOrDefaultAsync(rt => rt.TokenHash == tokenHash);
        }

        public async Task<int> RevokeAllAsync(long userId, string? revokedBy = "user", string? revokeReson = "user-wide revoke")
        {
            var now = DateTime.UtcNow;

            return await _context.RefreshTokens
                .Where(rt => rt.UserId == userId && rt.RevokedAt == null && rt.ExpiresAt > now)
                .ExecuteUpdateAsync(setters => setters
                    .SetProperty(rt => rt.RevokedAt, now)
                    .SetProperty(rt => rt.RevokedBy, revokedBy)
                    .SetProperty(rt => rt.RevokeReason, revokeReson)
                );
        }

        public async Task UpdateAsync(RefreshToken token)
        {
            _context.RefreshTokens.Update(token);
            await Task.CompletedTask;
        }
    }
}
