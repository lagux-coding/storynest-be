using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class AIUsageLog
    {
        public int Id { get; set; }

        public long UserId { get; set; }
        public User User { get; set; } = default!;

        public UsageFeature UsageFeature { get; set; } = UsageFeature.Other;
        public int InputToken { get; set; }
        public int OutputToken { get; set; }
        public int CreditUsed { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
