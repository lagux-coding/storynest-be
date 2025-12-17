using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Dtos.Response
{
    public class DashboardStoriesResponse
    {
        public PaginatedDefault<Story> Stories { get; set; }
    }
}
