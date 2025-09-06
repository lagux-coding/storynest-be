using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Request
{
    public class RevokeAllRequest
    {
        public string? RevokedBy { get; set; } = "user";
        public string? Reason { get; set; } = "user-wide revoke";
    }
}
