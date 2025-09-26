using StoryNest.Domain.Entities;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Persistence.Repositories
{
    public class AIUsageLogRepository : IAIUsageLogRepository
    {
        private readonly MyDbContext _context;

        public AIUsageLogRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AIUsageLog usage)
        {
            await _context.AIUsageLogs.AddAsync(usage);
        }
    }
}
