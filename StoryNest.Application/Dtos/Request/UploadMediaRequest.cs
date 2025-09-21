using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Request
{
    public class UploadMediaRequest
    {
        public string ResourceType { get; set; } = default!;
        public List<FileMetadata> Files { get; set; } = new();
    }
}
