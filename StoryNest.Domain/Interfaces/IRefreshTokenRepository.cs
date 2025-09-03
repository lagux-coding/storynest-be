using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Interfaces
{
    public interface IRefreshTokenRepository
    {
        Task<RefreshTokens> GetByHashAsync(string tokenHash);
        Task AddAsync(RefreshTokens token);
        Task UpdateAsync(RefreshTokens token);
    }
}
