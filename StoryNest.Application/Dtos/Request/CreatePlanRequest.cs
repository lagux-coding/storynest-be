using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Request
{
    public class CreatePlanRequest
    {
        public string Name { get; set; } = default!;
        public string Description { get; set; } = default!;
        public decimal PriceMonthly { get; set; }
        public List<string> Features { get; set; } = new List<string>();
        public Currency currency { get; set; } = Currency.VND;
        public int AiCreditsDaily { get; set; }
        public double DurationInDays { get; set; }
        public int SortOrder { get; set; } = 0;
    }
}
