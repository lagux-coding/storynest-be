using StoryNest.Application.Dtos.Response;
using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IStorySentimentAnalysisService
    {
        Task<int> CreateStorySentimentAnalysisAsync(int storyId, float score, float magnitude, string text, string source, string jobId);
        Task<PaginatedDefault<StorySentimentAnalysis>> GetAllAnalysisAsync(int page = 1, int pageSize = 10);
    }
}
