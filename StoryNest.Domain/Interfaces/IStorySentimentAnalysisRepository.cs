using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Interfaces
{
    public interface IStorySentimentAnalysisRepository
    {
        Task AddAsync(StorySentimentAnalysis analysis);
        Task<List<StorySentimentAnalysis>> GetStorySentimentAnalysesAsync(int page = 1, int pageSize = 10);
        Task<int> GetTotalCountAsync();
    }
}
