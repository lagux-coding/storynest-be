using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class RefreshToken
    {
        public Guid Id { get; set; }
        public long UserId { get; set; }
        public User User { get; set; } = default!;
        public string TokenHash { get; set; } = default!;
        public string JwtId { get; set; } = default!;
        public DateTime ExpiresAt { get; set; }        
        public DateTime CreatedAt { get; set; }
        public string? ReplacedByTokenHash { get; set; }

        // Revoke
        public DateTime? RevokedAt { get; set; }
        public string? RevokedBy { get; set; }
        public string? RevokeReason { get; set; }

        // Tracking
        public string? DeviceId { get; set; }
        public string? IpAddress { get; set; }
        public string? UserAgent { get; set; }

        public bool IsExpired => DateTime.UtcNow >= ExpiresAt;
        public bool IsActive => RevokedAt == null && !IsExpired;
    }
}
