using StoryNest.Domain.Entities;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Persistence.Repositories
{
    public class StorySentimentAnalysisRepository : IStorySentimentAnalysisRepository
    {
        private readonly MyDbContext _context;

        public StorySentimentAnalysisRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(StorySentimentAnalysis analysis)
        {
            await _context.StorySentimentAnalysis.AddAsync(analysis);
        }
    }
}
