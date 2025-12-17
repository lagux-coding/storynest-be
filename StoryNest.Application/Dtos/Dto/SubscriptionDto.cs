using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Dto
{
    public class SubscriptionDto
    {
        public int TotalSubscriptions { get; set; }
        public int ActiveSubscriptions { get; set; }
        public decimal TotalRevenue { get; set; }
        public int TotalFreemiumUsers { get; set; }
        public int TotalPremiumUsers { get; set; }
        public PlanCountDto? BloomPlan { get; set; }
        public PlanCountDto? FlourishPlan { get; set; }
        public PlanCountDto? EnsemblePlan { get; set; }
    }
}
