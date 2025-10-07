using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Dto
{
    public class InvoiceDto
    {
        public long OrderCode { get; set; } = 1234567890;
        public int Amount { get; set; } = 50000;
        public DateTime IssueDate { get; set; } = DateTime.UtcNow;
        public User User { get; set; }
        public Subscription Subscription { get; set; }
        public Payment Payment { get; set; }
    }
}
