using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class Follow
    {
        public int Id { get; set; }

        public long UserId { get; set; }
        public User User { get; set; } = default!;

        public long FollowerId { get; set; }
        public User Follower { get; set; } = default!;

        public FollowStatus FollowStatus { get; set; } = FollowStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
