using StackExchange.Redis;
using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Services.Redis
{
    public class RedisService : IRedisService
    {
        private readonly IDatabase _db;

        public RedisService(IConnectionMultiplexer redis)
        {
            _db = redis.GetDatabase();
        }

        public async Task SetBlacklistAsync(string jti, TimeSpan expiry)
        {
            await _db.StringSetAsync($"reset_blacklist:{jti}", "revoked", expiry);
        }

        public async Task<bool> IsBlacklistedAsync(string jti)
        {
            return await _db.KeyExistsAsync($"reset_blacklist:{jti}");
        }
    }
}
