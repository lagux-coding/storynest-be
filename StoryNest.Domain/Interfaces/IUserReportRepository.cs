using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Interfaces
{
    public interface IUserReportRepository
    {
        Task CreateUserReportAsync(UserReport report);
        Task UpdateUserReportAsync(UserReport report);
        Task<List<UserReport>> GetAllPendingReportsAsync();
        Task<List<UserReport>> GetAllUserReports(ReportType type, int page = 1, int pageSize = 10);
        Task<int> CountReportsAsync(ReportType type = ReportType.Story);
    }
}
