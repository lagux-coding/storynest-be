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
        Task SendNotificationAsync(long userId, long? actorId, string content, NotificationType type, int? referenceId = null, string? referenceType = null);
    }
}
