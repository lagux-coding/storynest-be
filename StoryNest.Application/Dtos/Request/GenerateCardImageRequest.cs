using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Request
{
    public class GenerateCardImageRequest
    {
        public string Content { get; set; } = null!;
        public string? Style { get; set; }
        public int Width { get; set; } = 1024;
        public int Height { get; set; } = 1024;
    }
}
