using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class Comment
    {
        public int Id { get; set; }

        public int StoryId { get; set; }
        public Story Story { get; set; } = default!;

        public long UserId { get; set; } = default!;
        public User User { get; set; } = default!;

        public int? ParentCommentId { get; set; }
        public string Content { get; set; } = null!;
        public CommentStatus CommentStatus { get; set; } = CommentStatus.Active;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }
    }
}
