using Microsoft.AspNetCore.SignalR;
using StoryNest.API.Hubs;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Interfaces;

namespace StoryNest.API.Services
{
    public class NotificationHubService : INotificationHubService
    {
        private readonly IHubContext<NotificationHub, INotificationClient> _hub;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUnitOfWork _unitOfWork;

        public NotificationHubService(
            IHubContext<NotificationHub, INotificationClient> hub,
            INotificationRepository notificationRepository,
            IUnitOfWork unitOfWork)
        {
            _hub = hub;
            _notificationRepository = notificationRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task SendNotificationAsync(long userId, NotificationResponse notification)
        {
            await _hub.Clients.User(userId.ToString()).ReceiveNotification(notification);
        }

        public async Task MarkAllAsReadAsync(long userId)
        {
            await _notificationRepository.MarkAllAsReadAsync(userId);
            await _unitOfWork.SaveAsync();

            await _hub.Clients.User(userId.ToString()).NotificationsMarkedAsRead();
        }
    }
}
