using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Response
{
    public class StoryPreviewResponse
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Summary { get; set; } = string.Empty; // preview text
        public string CoverImageUrl { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
