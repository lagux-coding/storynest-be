using StoryNest.Domain.Entities;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Persistence.Repositories
{
    public class AITransactionRepository : IAITransactionRepository
    {
        private readonly MyDbContext _context;

        public AITransactionRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(AITransaction transaction)
        {
            await _context.AITransactions.AddAsync(transaction);
        }
    }
}
