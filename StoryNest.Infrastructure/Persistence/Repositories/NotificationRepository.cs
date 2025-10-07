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
    public class NotificationRepository : INotificationRepository
    {
        private readonly MyDbContext _context;

        public NotificationRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddNotificationAsync(Notification notification)
        {
            _context.Notifications.Add(notification);
        }

        public async Task MarkAllAsReadAsync(long userId)
        {
            await _context.Notifications
                .Where(n => n.UserId == userId && !n.IsRead)
                .ExecuteUpdateAsync(n => n.SetProperty(x => x.IsRead, true).SetProperty(x => x.ReadAt, DateTime.UtcNow));
        }

        public async Task<Notification?> GetWithRelationsAsync(int id)
        {
            return await _context.Notifications
                .Include(n => n.User)
                .Include(n => n.Actor)
                .FirstOrDefaultAsync(n => n.Id == id);
        }

    }
}
