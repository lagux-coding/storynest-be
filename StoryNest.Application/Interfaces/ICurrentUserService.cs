using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface ICurrentUserService
    {
        long? UserId { get; }
        string? Username { get; }
        string? Email { get; }
        string? Type { get; }
        string IpAddress { get; }
    }
}
