using StoryNest.Domain.Entities;
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
    }
}
