using StoryNest.Domain.Entities;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Persistence.Repositories
{
    public class PlanRepository : IPlanRepository
    {
        private readonly MyDbContext _context;

        public PlanRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Plan plan)
        {
            await _context.AddAsync(plan);
        }
    }
}
