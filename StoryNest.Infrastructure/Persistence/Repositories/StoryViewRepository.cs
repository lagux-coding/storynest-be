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
    public class StoryViewRepository : IStoryViewRepository
    {
        private readonly MyDbContext _context;

        public StoryViewRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddStoryViewLogAsync(int storyId, long userId, string? ip = null, string? device = null)
        {
            var recent = await _context.StoryViews
                .Where(v => v.StoryId == storyId && v.UserId == userId)
                .OrderByDescending(v => v.ViewedAt)
                .FirstOrDefaultAsync();

            if (recent != null && (DateTime.UtcNow - recent.ViewedAt).TotalHours < 6)
                return;

            var log = new StoryView
            {
                StoryId = storyId,
                UserId = userId,
                IpAddress = ip ?? string.Empty,
                DeviceInfo = device ?? string.Empty,
                ViewedAt = DateTime.UtcNow
            };

            await _context.AddAsync(log);
        }
    }
}
