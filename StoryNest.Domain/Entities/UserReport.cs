using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class UserReport
    {
        public int Id { get; set; }

        public long ReportedId { get; set; }
        public User? ReportedUser { get; set; }

        public long ReporterId { get; set; }
        public User Reporter { get; set; }

        public int? ReportedStoryId { get; set; }
        public Story? ReportedStory { get; set; }

        public int? ReportedCommentId { get; set; }
        public Comment? ReportedComment { get; set; }

        public int? AdminId { get; set; }
        public Admin? Admin { get; set; }

        public string Reason { get; set; } = string.Empty;
        public ReportStatus Status{ get; set; } = ReportStatus.Pending;       
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
