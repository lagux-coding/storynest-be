using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IJwtService
    {
        public string GenerateAccessToken(long userId, string username, string email, string type, out string jwtId);
        public string GenerateRefreshToken();
        public Task<string> GenerateResetPasswordToken(User user);
        ClaimsPrincipal? GetPrincipalFromExpiredToken(string token);
        Task<ClaimsPrincipal?> VerifyResetPasswordToken(string token);
    }
}
