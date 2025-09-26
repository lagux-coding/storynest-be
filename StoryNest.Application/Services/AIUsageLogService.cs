using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Services
{
    public class AIUsageLogService : IAIUsageLogService
    {
        private readonly IAIUsageLogRepository _aiUsageLogRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AIUsageLogService(IAIUsageLogRepository aiUsageLogRepository, IUnitOfWork unitOfWork)
        {
            _aiUsageLogRepository = aiUsageLogRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> AddUsageAsync(long userId, UsageFeature feature, int inputToken, int outputToken, int creditUsed)
        {
            try
            {
                var usage = new AIUsageLog
                {
                    UserId = userId,
                    UsageFeature = feature,
                    InputToken = inputToken,
                    OutputToken = outputToken,
                    CreditUsed = creditUsed,
                    CreatedAt = DateTime.UtcNow,
                };

                await _aiUsageLogRepository.AddAsync(usage);
                return await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
