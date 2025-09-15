using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class StoryTag
    {
        public int Id { get; set; }

        public int StoryId { get; set; }
        public Story Story { get; set; } = default!;

        public int TagId { get; set; }
        public Tag Tag { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
