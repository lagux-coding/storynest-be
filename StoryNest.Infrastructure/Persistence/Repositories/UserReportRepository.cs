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

        public async Task<int> CountReportsAsync(ReportType type = ReportType.Story)
        {
            return await _context.UserReports
                .Where(r => r.Type == type)
                .CountAsync();
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

        public async Task<List<UserReport>> GetAllUserReports(ReportType type, int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            return await _context.UserReports
                .Include(r => r.ReportedUser)
                .Include(r => r.Reporter)
                .Where(r => r.Type == type)
                .OrderByDescending(r => r.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task UpdateUserReportAsync(UserReport report)
        {
            _context.UserReports.Update(report);
            await Task.CompletedTask;
        }
    }
}
