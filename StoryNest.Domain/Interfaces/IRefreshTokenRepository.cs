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
        Task<RefreshToken> GetByHashAsync(string tokenHash);
        Task AddAsync(RefreshToken token);
        Task UpdateAsync(RefreshToken token);
        Task<int> RevokeAllAsync(long userId, string? revokedBy = "user", string? reason = "user-wide revoke");
    }
}
