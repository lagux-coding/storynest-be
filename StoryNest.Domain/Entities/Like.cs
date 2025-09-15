using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class Like
    {
        public int Id { get; set; }

        public int StoryId { get; set; }
        public Story Story { get; set; } = default!;

        public long UserId { get; set; }
        public User User { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? RevokedAt { get; set; }
    }
}
