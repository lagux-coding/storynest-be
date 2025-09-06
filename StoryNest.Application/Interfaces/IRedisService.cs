using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IRedisService
    {
        Task SetBlacklistAsync(string jti, TimeSpan expiry);
        Task<bool> IsBlacklistedAsync(string jti);
    }
}
