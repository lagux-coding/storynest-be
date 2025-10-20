using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class Notification
    {
        public int Id { get; set; }

        public long UserId { get; set; }
        public User User { get; set; } = default!;

        public long? ActorId { get; set; }
        public User? Actor { get; set; } = default!;

        public int? ReferenceId { get; set; }
        public string? ReferenceType { get; set; }

        public string Content { get; set; } = string.Empty;
        public NotificationType Type { get; set; } = NotificationType.System;

        public bool IsRead { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ReadAt { get; set; }
        public bool IsAnonymous { get; set; } = false;
    }
}
