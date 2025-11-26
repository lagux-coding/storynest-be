using StoryNest.Application.Dtos.Dto;
using StoryNest.Application.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardDto> GetDashboardStatsAsync();
        Task<SubscriptionDto> GetSubscriptionStatsAsync();
        Task<DashboardStoriesResponse> GetDashboardStoryAsync(int page = 1, int pageSize = 10);
    }
}
