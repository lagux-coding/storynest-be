using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Request
{
    public class LogoutRequest
    {
        public string RefreshToken { get; set; } = default!;
    }
}
