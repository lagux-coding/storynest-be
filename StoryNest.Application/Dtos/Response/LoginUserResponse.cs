using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Response
{
    public class LoginUserResponse
    {
        public string? Username { get; set; }
        public string? AvatarUrl { get; set; }
        public string? PlanName { get; set; }
        public int? PlanId { get; set; }
        public string? AccessToken { get; set; }
        public string? RefreshToken { get; set; }
    }
}
