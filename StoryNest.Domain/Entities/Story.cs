using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class Story
    {
        public int Id { get; set; }
        public string Title { get; set; } = null!;
        public string Slug { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? Summary { get; set; }
        public string? CoverImageUrl { get; set; }
        public PrivacyStatus PrivacyStatus { get; set; } = PrivacyStatus.Public;
        public StoryStatus StoryStatus { get; set; } = StoryStatus.Draft;
        public int ViewCount { get; set; } = 0;
        public int LikeCount { get; set; } = 0;
        public int CommentCount { get; set; } = 0;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastUpdatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }

        // Relations
        public ICollection<Media> Media { get; set; } = new List<Media>();
        public ICollection<CollectionStory> CollectionStories { get; set; } = new List<CollectionStory>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<StoryTag> StoryTags { get; set; } = new List<StoryTag>();
        public ICollection<StoryView> StoryViews { get; set; } = new List<StoryView>();
        public ICollection<UserReport> Reports { get; set; } = new List<UserReport>();

    }
}
