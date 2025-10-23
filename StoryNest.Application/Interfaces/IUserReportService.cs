using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IUserReportService
    {
        Task<int> CreateReportAsync(UserReportRequest request, long reporterId, int storyId, ReportType type = ReportType.Story, int commentId = 0);
        Task<List<UserReport>> GetAllPendingReportsAsync();
        Task<PaginatedDefault<UserReportResponse>> GetAllUserReport(ReportType type = ReportType.Story, int page = 1, int pageSize = 10);
        Task<int> UpdateUserReportAsync(UserReport report);
    }
}
