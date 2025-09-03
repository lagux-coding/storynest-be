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

        public Task<RefreshTokens> GetByHashAsync(string tokenHash)
        {
            throw new NotImplementedException();
        }

        public Task UpdateAsync(RefreshTokens token)
        {
            throw new NotImplementedException();
        }
    }
}
