using Microsoft.EntityFrameworkCore;
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

        public async Task<AICredit?> GetById(int id)
        {
            return await _context.AICredits.FirstOrDefaultAsync(a => a.Id == id);
        }

        public async Task<AICredit?> GetByUserId(long userId)
        {
            return await _context.AICredits.FirstOrDefaultAsync(a => a.UserId == userId);
        }

        public void UpdateAsync(AICredit credit)
        {
            _context.AICredits.Update(credit);
        }
    }
}
