using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class AICredit
    {
        public int Id { get; set; }

        public long UserId { get; set; }
        public User User { get; set; }

        public int TotalCredits { get; set; }
        public int UsedCredits { get; set; }
        public int RemainingCredits => TotalCredits - UsedCredits;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? LastRenewDate { get; set; }
        }
}
