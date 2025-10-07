using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Request
{
    public class FileMetadata
    {
        public string ContentType { get; set; } = default!;
        public long FileSize { get; set; }
    }
}
