using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Request
{
    public class ConfirmUploadRequest
    {
        public string ResourceType { get; set; } = default!;
        public int? ResourceId { get; set; }
        public List<string> FileKeys { get; set; } = new(); 
    }
}
