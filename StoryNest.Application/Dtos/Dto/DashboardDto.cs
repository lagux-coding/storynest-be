using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Dto
{
    public class DashboardDto
    {
        public int TotalUsers { get; set; }
        public int TotalStories { get; set; }
        public int TotalComments { get; set; }
        public int TotalSubscriptions { get; set; }
    }
}
