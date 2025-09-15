using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class CollectionStory
    {
        public int Id { get; set; }

        public int CollectionId { get; set; }
        public Collection Collection { get; set; } = default!;

        public int StoryId { get; set; }
        public Story Story { get; set; } = default!;

        public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    }
}
