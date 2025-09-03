using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Request
{
    public class LoginUserRequest
    {
        public string UsernameOrEmail { get; set; } = default!;
        public string Password { get; set; } = default!;
        
        // Optional
        public string? DeviceId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }
    }
}
