using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Response
{
    public class PaginatedResponse<T>
    {
        public string? NextCursor { get; set; }
        public bool HasMore { get; set; }
        public IEnumerable<T> Items { get; set; } = new List<T>();
    }
}
