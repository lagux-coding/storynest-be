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
        public bool Remember { get; set; } = false;

        // Optional
        public string? DeviceId { get; set; } = string.Empty;
        public string? IpAddress { get; set; } = string.Empty;
        public string? UserAgent { get; set; } = string.Empty;
    }
}
