using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Request
{
    public class RegisterUserRequest
    {
        public string Username { get; set; } = default!;
        public string Email { get; set; } = default!;
        public string? FullName { get; set; }
        public string Password { get; set; } = default!;
        public string ConfirmPassword { get; set; } = default!;
    }
}
