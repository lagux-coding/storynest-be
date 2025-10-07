using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Response
{
    public class CommentResponse
    {
        // 1. Identity
        public int Id { get; set; }
        public int StoryId { get; set; }
        public long UserId { get; set; }
        public int? ParentCommentId { get; set; }

        public string Content { get; set; } = null!;
        public CommentStatus CommentStatus { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        public UserBasicResponse User { get; set; } = default!;

        public int RepliesCount { get; set; } = 0;
        public bool HasReplies { get; set; } = false;
    }
}
