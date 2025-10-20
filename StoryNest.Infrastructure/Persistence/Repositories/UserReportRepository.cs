using Microsoft.EntityFrameworkCore;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Persistence.Repositories
{
    public class UserReportRepository : IUserReportRepository
    {
        private readonly MyDbContext _context;

        public UserReportRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task CreateUserReportAsync(UserReport report)
        {
            await _context.UserReports.AddAsync(report);
        }

        public async Task<List<UserReport>> GetAllPendingReportsAsync()
        {
            var rawReports = await _context.UserReports
                .Include(r => r.ReportedStory)
                .Where(r => r.Status == ReportStatus.Pending &&
                            r.ReportedStoryId != null &&
                            !_context.StorySentimentAnalysis.Any(s => s.StoryId == r.ReportedStoryId))
                .ToListAsync(); // <- load trước từ DB

            // distinct theo storyId (lọc trùng ở memory)
            var distinctReports = rawReports
                .GroupBy(r => r.ReportedStoryId)
                .Select(g => g.First())
                .Take(10)
                .ToList();

            return distinctReports;
        }

        public async Task UpdateUserReportAsync(UserReport report)
        {
            _context.UserReports.Update(report);
            await Task.CompletedTask;
        }
    }
}
