using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Response
{
    public class LoginUserResponse
    {
        public string Token { get; set; } = default!;
        public DateTime Expiration { get; set; }
    }
}
