using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class StoryView
    {
        public int Id { get; set; }

        public int StoryId { get; set; }
        public Story Story { get; set; } = default!;

        public long? UserId { get; set; }
        public User? User { get; set; }

        public string IpAddress { get; set; } = string.Empty;
        public string DeviceInfo { get; set; } = string.Empty;
        public DateTime ViewedAt { get; set; } = DateTime.UtcNow;
    }
}
