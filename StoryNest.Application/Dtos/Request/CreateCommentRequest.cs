using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Request
{
    public class CreateCommentRequest
    {
        public string Content { get; set; } = null!;
        public bool IsAnonymous { get; set; } = false;
        public int? ParentCommentId { get; set; }
    }
}
