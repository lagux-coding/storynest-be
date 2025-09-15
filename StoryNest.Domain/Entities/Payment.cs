using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class Payment
    {
        public int Id { get; set; }

        public long UserId { get; set; }
        public User User { get; set; } = default!;

        public int SubscriptionId { get; set; }
        public Subscription Subscription { get; set; } = default!;

        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string Provider { get; set; } = string.Empty;
        public string ProviderTXN { get; set; } = string.Empty;
        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;
        public DateTime PaidAt { get; set; } = DateTime.UtcNow;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
