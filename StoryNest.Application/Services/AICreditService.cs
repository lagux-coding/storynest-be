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
    public class AICreditService : IAICreditService
    {
        private readonly IAICreditRepository _creditRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AICreditService(IAICreditRepository creditRepository, IUnitOfWork unitOfWork)
        {
            _creditRepository = creditRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task AddCreditsAsync(long userId, int amount)
        {
            try
            {
                var credit = new AICredit
                {
                    UserId = userId,
                    TotalCredits = amount,
                    CreatedAt = DateTime.UtcNow
                };

                await _creditRepository.AddAsync(credit);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw new Exception(ex.InnerException?.Message);
            }
        }
    }
}
