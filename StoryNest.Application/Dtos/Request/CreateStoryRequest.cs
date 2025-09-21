using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Request
{
    public class CreateStoryRequest
    {
        public string Title { get; set; } = null!;
        public string Content { get; set; } = null!;
        public string? CoverImageUrl { get; set; }
        public List<string> Tags { get; set; } = new();
        public PrivacyStatus PrivacyStatus { get; set; } = PrivacyStatus.Public;
        public StoryStatus StoryStatus { get; set; } = StoryStatus.Draft;

        public List<string> MediaUrls { get; set; } = new();

    }
}
