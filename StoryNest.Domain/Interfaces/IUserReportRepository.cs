using StoryNest.Domain.Entities;
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
    }
}
