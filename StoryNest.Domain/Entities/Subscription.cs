using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class Subscription
    {
        public int Id { get; set; }

        public long UserId { get; set; }
        public User User { get; set; }

        public int PlanId { get; set; }
        public Plan Plan { get; set; }

        public DateTime StartDate { get; set; } = DateTime.UtcNow;
        public DateTime EndDate { get; set; }
        public SubscriptionStatus Status { get; set; } = SubscriptionStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;

        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    }
}
