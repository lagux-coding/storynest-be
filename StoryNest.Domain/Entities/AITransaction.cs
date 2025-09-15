using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class AITransaction
    {
        public int Id { get; set; }
        
        public long UserId { get; set; }
        public User User { get; set; } = default!;

        public AITransactionType Type { get; set; }
        public int Amount { get; set; }
        public string? Description { get; set; }
        public int BalanceAfter { get; set; }
        public int ReferenceId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
