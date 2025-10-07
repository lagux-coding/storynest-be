using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Interfaces;
using System.Security.Claims;

namespace StoryNest.API.Hubs
{
    public interface INotificationClient
    {
        Task ReceiveNotification(NotificationResponse notification);
        Task NotificationsMarkedAsRead();
    }

    [Authorize]
    public class NotificationHub : Hub<INotificationClient>
    {
        private readonly INotificationHubService _notificationHubService;

        public NotificationHub(INotificationHubService notificationHubService)
        {
            _notificationHubService = notificationHubService;
        }

        public override Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"User connected: {userId}, ConnectionId: {Context.ConnectionId}");
            return base.OnConnectedAsync();
        }

        public override Task OnDisconnectedAsync(Exception? ex)
        {
            var userId = Context.User?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            Console.WriteLine($"User disconnected: {userId}, ConnectionId: {Context.ConnectionId}");
            return base.OnDisconnectedAsync(ex);
        }

        public async Task MarkAllAsRead()
        {
            var userId = long.Parse(Context.UserIdentifier!);
            await _notificationHubService.MarkAllAsReadAsync(userId);
        }
    }
}
