using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Response
{
    public class LoginAdminResponse
    {
        public long Id { get; set; }
        public string? Username { get; set; }
        public string? AvatarUrl { get; set; }
        public string? AccessToken { get; set; }
    }
}
