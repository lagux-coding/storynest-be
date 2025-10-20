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
    }
}
