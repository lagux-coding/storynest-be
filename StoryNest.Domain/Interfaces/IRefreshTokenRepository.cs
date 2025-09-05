using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshTokens> GetByHashAsync(string tokenHash);
        Task AddAsync(RefreshTokens token);
        Task UpdateAsync(RefreshTokens token);
        Task<int> RevokeAllAsync(long userId, string? revokedBy = "user", string? reason = "user-wide revoke");
    }
}
