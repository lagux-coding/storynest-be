using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class Collection
    {
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string? CoverImageUrl { get; set; }
        public Boolean IsPublic { get; set; } = true;
        public int StoryCount { get; set; } = 0;

        public long UserId { get; set; }
        public User User { get; set; } = default!;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
        public DateTime? DeletedAt { get; set; }

        // Relations
        public ICollection<CollectionStory> CollectionStories { get; set; } = new List<CollectionStory>();
    }
}
