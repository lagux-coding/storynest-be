using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Response
{
    public class StorySearchResult
    {
        public List<StoryResponse> Stories { get; set; } = new List<StoryResponse>();
        public int? LastId { get; set; }
        public bool HasMore { get; set; }
    }
}
