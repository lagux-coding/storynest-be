using Microsoft.AspNetCore.Http;
using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Services.User
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
    }
}
