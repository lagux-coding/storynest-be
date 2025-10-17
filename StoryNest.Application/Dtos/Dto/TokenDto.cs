using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Dto
{
    public class TokenDto
    {
        public int Index { get; set; }
        public string WordForm { get; set; } = "";
        public string PosTag { get; set; } = "";
        public string NerLabel { get; set; } = "";
        public int Head { get; set; }
        public string DepLabel { get; set; } = "";
    }
        
    public class Root
    {
        public bool Success { get; set; }
        public Dictionary<string, List<TokenDto>> Data { get; set; } = new();
    }
}
