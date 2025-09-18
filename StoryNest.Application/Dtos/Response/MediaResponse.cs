using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Response
{
    public class MediaResponse
    {
        public int Id { get; set; }
        public int MediaId { get; set; }
        public string MediaUrl { get; set; } = string.Empty;        
        public string MediaType { get; set; } = string.Empty;
        public string Caption { get; set; } = string.Empty;
        public string MimeType { get; set; } = string.Empty;
        public int Size { get; set; }
        public int Width { get; set; }
        public int Height { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
