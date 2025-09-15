using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Entities
{
    public class YearlyMemory
    {
        public int Id { get; set; }

        public long UserId { get; set; }
        public User User { get; set; } = default!;

        public int Year { get; set; }
        public string MediaUrl { get; set; } = string.Empty;
        public YearlyMemoryStatus Status { get; set; } = YearlyMemoryStatus.Processing;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; } = DateTime.UtcNow;
    }
}
