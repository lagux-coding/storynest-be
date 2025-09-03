using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IJwtService
    {
        public string GenerateAccessToken(long userId, string username, string email, string type);
        public string GenerateRefreshToken();
    }
}
