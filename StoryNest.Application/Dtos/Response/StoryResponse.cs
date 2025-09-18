using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Response
{
    public class StoryResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string CoverImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public DateTime? PublishedAt { get; set; }
        public int LikeCount { get; set; }
        public int CommentCount { get; set; }
        public PrivacyStatus PrivacyStatus { get; set; }
        public StoryStatus StoryStatus { get; set; }
        public UserBasicResponse User { get; set; }
        public List<MediaResponse> Media { get; set; } = new List<MediaResponse>();
        public List<TagResponse> Tags { get; set; } = new List<TagResponse>();
    }
}
