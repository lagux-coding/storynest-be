using AutoMapper;
using Microsoft.AspNetCore.SignalR;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly INotificationRepository _notificationRepository;
        private readonly IUserService _userService;
        private readonly IMapper _mapper;
        private readonly INotificationHubService _hubService;

        public NotificationService(IUnitOfWork unitOfWork, INotificationRepository notificationRepository, IMapper mapper, INotificationHubService hubService, IUserService userService)
        {
            _unitOfWork = unitOfWork;
            _notificationRepository = notificationRepository;
            _mapper = mapper;
            _hubService = hubService;
            _userService = userService;
        }

        public async Task SendNotificationAsync(long userId, long? actorId, string content, NotificationType type, int? referenceId = null, string? referenceType = null)
        {
            try
            {
                var noti = new Notification
                {
                    UserId = userId,
                    ActorId = actorId,
                    ReferenceId = referenceId,
                    ReferenceType = referenceType,
                    Content = content,
                    Type = type,
                    IsRead = false,
                    CreatedAt = DateTime.UtcNow
                };

                await _notificationRepository.AddNotificationAsync(noti);
                await _unitOfWork.SaveAsync();

                var fullNoti = await _notificationRepository.GetWithRelationsAsync(noti.Id);

                var dto = _mapper.Map<NotificationResponse>(fullNoti);

                await _hubService.SendNotificationAsync(userId, dto);
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
