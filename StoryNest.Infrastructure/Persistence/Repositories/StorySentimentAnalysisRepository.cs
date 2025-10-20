using Microsoft.EntityFrameworkCore;
using QuestPDF.Helpers;
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

        public async Task<List<StorySentimentAnalysis>> GetStorySentimentAnalysesAsync(int page = 1, int pageSize = 10)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 10;
            return await _context.StorySentimentAnalysis
                .OrderByDescending(s => s.AnalyzedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();
        }

        public async Task<int> GetTotalCountAsync()
        {
            return await _context.StorySentimentAnalysis.CountAsync();
        }
    }
}
