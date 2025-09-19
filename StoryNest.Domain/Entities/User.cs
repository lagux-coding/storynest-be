using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class User
    {
        public long Id { get; set; }
        public string Username { get; set; } = null!;
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;

        // Profile
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }
        public DateTime? DateOfBirth { get; set; }
        public string? Gender { get; set; }

        // Systems
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public bool IsActive { get; set; } = true;

        public ICollection<RefreshToken> RefreshTokens { get; set; } = new List<RefreshToken>();
        public ICollection<Subscription> Subscriptions { get; set; } = new List<Subscription>();
        public ICollection<AITransaction> AITransactions { get; set; } = new List<AITransaction>();

        public ICollection<Story> Stories { get; set; } = new List<Story>();

        public ICollection<Like> Likes { get; set; } = new List<Like>();
        public ICollection<Comment> Comments { get; set; } = new List<Comment>();

        // Who follow me
        public ICollection<Follow> Follows { get; set; } = new List<Follow>();
        // Who I follow
        public ICollection<Follow> Following { get; set; } = new List<Follow>();

        public ICollection<AIUsageLog> AIUsageLogs { get; set; } = new List<AIUsageLog>();
        public ICollection<Collection> Collections { get; set; } = new List<Collection>();
        public ICollection<Notification> Notifications { get; set; } = new List<Notification>();
        public ICollection<Notification> ActorNotifications { get; set; } = new List<Notification>();
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ICollection<StoryView> StoryViews { get; set; } = new List<StoryView>();
        public ICollection<UserReport> ReportsCreated { get; set; } = new List<UserReport>();
        public ICollection<UserReport> ReportsReceived { get; set; } = new List<UserReport>();
        public ICollection<YearlyMemory> YearlyMemories { get; set; } = new List<YearlyMemory>();

        // 1 - 1 relations
        public AICredit AICredit { get; set; } = default!;
    }
}
