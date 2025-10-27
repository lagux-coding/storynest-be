using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Request
{
    public class CheckOffensiveRequest
    {
        public string? Title { get; set; }
        public string? Content { get; set; }
        public List<string> Tags { get; set; } = new();
    }
}
