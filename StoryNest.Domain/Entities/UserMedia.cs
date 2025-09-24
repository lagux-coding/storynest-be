using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class UserMedia
    {
        public int Id { get; set; }
        public long UserId { get; set; }
        public MediaType MediaType { get; set; } = MediaType.Image;
        public string MediaUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        public UserMediaStatus Status { get; set; } = UserMediaStatus.Pending;

        // Navigation property
        public User User { get; set; } = null!;
    }
}
