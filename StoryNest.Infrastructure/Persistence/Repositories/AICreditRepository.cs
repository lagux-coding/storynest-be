using StoryNest.Domain.Entities;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Persistence.Repositories
{
    public class AICreditRepository : IAICreditRepository
    {
        private readonly MyDbContext _context;

        public AICreditRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AICredit credit)
        {
            await _context.AICredits.AddAsync(credit);
        }
    }
}
