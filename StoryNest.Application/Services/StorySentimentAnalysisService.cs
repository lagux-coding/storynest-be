using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Services
{
    public class StorySentimentAnalysisService : IStorySentimentAnalysisService
    {
        private readonly IStorySentimentAnalysisRepository _storySentimentAnalysisRepository;
        private readonly IUnitOfWork _unitOfWork;

        public StorySentimentAnalysisService(IStorySentimentAnalysisRepository storySentimentAnalysisRepository, IUnitOfWork unitOfWork)
        {
            _storySentimentAnalysisRepository = storySentimentAnalysisRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> CreateStorySentimentAnalysisAsync(int storyId, float score, float magnitude, string text, string source, string jobId)
        {
            try
            {
                var sentimentAnalysis = new StorySentimentAnalysis
                {
                    StoryId = storyId,
                    Score = score,
                    Magnitude = magnitude,
                    AnalyzedText = text,
                    Source = source,
                    JobId = jobId,
                    AnalyzedAt = DateTime.UtcNow,
                    IsSuccessful = true,
                    ErrorMessage = "Ok"
                };

                await _storySentimentAnalysisRepository.AddAsync(sentimentAnalysis);
                return await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw;
            }

        }

        public async Task<PaginatedDefault<StorySentimentAnalysis>> GetAllAnalysisAsync(int page = 1, int pageSize = 10)
        {
            try
            {

                var totalCount = await _storySentimentAnalysisRepository.GetTotalCountAsync();
                var items = await _storySentimentAnalysisRepository.GetStorySentimentAnalysesAsync(page, pageSize);
                var totalPages = (int)Math.Ceiling((double)totalCount / pageSize);
                return new PaginatedDefault<StorySentimentAnalysis>
                {
                    Items = items,
                    Page = page,
                    PageSize = pageSize,
                    TotalItems = totalCount,
                    TotalPages = totalPages
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
