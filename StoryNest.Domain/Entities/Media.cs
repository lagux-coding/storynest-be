using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class Media
    {
        public int Id { get; set; }
        
        public int StoryId { get; set; }
        public Story Story { get; set; } = default!;

        public string MediaUrl { get; set; } = null!;
        public MediaType MediaType { get; set; } = MediaType.Image;
        public string? Caption { get; set; }

        public string? MimeType { get; set; }
        public int? Size { get; set; }
        public int? Width { get; set; }
        public int? Height { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}
