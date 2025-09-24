using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Request
{
    public class GenerateCardVoiceRequest
    {
        public string Content { get; set; } = null!;
        public string? VoiceStyle { get; set; }
        public string Language { get; set; } = "vi-VN";
    }
}
