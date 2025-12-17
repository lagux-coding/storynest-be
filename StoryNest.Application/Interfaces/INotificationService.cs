using StoryNest.Application.Dtos.Response;
using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface INotificationService
    {
        Task<PaginatedResponse<NotificationResponse>> GetAllNotificationsAsync(long userId, int limit, long cursor = 0);
        Task SendNotificationAsync(long userId, string slug, long? actorId, string content, NotificationType type, int? referenceId = null, string? referenceType = null, bool isAnonymous = false);
    }
}
