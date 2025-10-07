using StoryNest.Application.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface INotificationHubService
    {
        Task SendNotificationAsync(long userId, NotificationResponse notification);
        public Task MarkAllAsReadAsync(long userId);
    }
}
