using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Response
{
    public class NotificationResponse
    {
        public long Id { get; set; }
        public long UserId { get; set; }
        public UserBasicResponse User { get; set; } = new UserBasicResponse();
        //public long? ActorId { get; set; }
        public UserBasicResponse? Actor { get; set; } = new UserBasicResponse();
        public int? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }
        public string Content { get; set; } = string.Empty;
        public NotificationType Type { get; set; }
        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; }
    }
}
