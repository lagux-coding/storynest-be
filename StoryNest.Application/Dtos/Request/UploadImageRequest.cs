using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Request
{
    public class UploadImageRequest
    {
        public string ResourceType { get; set; } = default!;
        public int? ResourceId { get; set; }
        public List<FileMetadata> Files { get; set; } = new();
    }
}
