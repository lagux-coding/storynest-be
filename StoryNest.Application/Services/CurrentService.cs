using Microsoft.AspNetCore.Http;
using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Services
{
    public class CurrentUserService : ICurrentUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CurrentUserService(IHttpContextAccessor httpContextAccessor)
        {
            _httpContextAccessor = httpContextAccessor;
        }

        public string IpAddress
        {
            get
            {
                var context = _httpContextAccessor.HttpContext;
                if (context == null) return null;

                if (context.Request.Headers.ContainsKey("X-Forwarded-For"))
                {
                    return context.Request.Headers["X-Forwarded-For"].FirstOrDefault()?.Split(',')[0];
                }

                return context.Connection.RemoteIpAddress?.ToString();
            }
        }

        public long? UserId
        {
            get
            {
                var userIdClaim = _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.NameIdentifier);

                return long.TryParse(userIdClaim, out var id) ? id : null;
            }
        }

        public string? Username =>
        _httpContextAccessor.HttpContext?.User.FindFirstValue("unique_name");

        public string? Email =>
            _httpContextAccessor.HttpContext?.User.FindFirstValue(ClaimTypes.Email);

        public string? Type => _httpContextAccessor.HttpContext?.User.FindFirstValue("type");
    }
}
