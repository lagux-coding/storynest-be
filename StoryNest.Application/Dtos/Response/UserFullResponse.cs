using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Response
{
    public class UserFullResponse : UserBasicResponse
    {
        public string Email { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? CoverUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string Gender { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int FollowersCount { get; set; } = 0;
    }
}
