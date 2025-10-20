using StoryNest.Application.Dtos.Request;
using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IUserReportService
    {
        Task<int> CreateReportAsync(UserReportRequest request, long reporterId, int storyId, int commentId = 0);
        Task<List<UserReport>> GetAllPendingReportsAsync();
        Task<int> UpdateUserReportAsync(UserReport report);
    }
}
