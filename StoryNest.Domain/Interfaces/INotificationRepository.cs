using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Interfaces
{
    public interface INotificationRepository
    {
        Task AddNotificationAsync(Notification notification);
        Task MarkAllAsReadAsync(long userId);
        Task<Notification?> GetWithRelationsAsync(int id);
        Task<List<Notification>> GetAllByUserId(long userId, int limit, long cursor = 0);
    }
}
