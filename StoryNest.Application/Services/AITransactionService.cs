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
    public class AITransactionService : IAITransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IAITransactionRepository _aiTransactionRepository;
        private readonly IAICreditService _aiCreditService;

        public AITransactionService(IUnitOfWork unitOfWork, IAITransactionRepository aiTransactionRepository, IAICreditService aiCreditService)
        {
            _unitOfWork = unitOfWork;
            _aiTransactionRepository = aiTransactionRepository;
            _aiCreditService = aiCreditService;
        }

        public async Task<int> AddTransactionAsync(long userId, int referenceId, int amount, string desc, AITransactionType type)
        {
            try
            {
                var credit = await _aiCreditService.GetUserCredit(userId);
                var transaction = new AITransaction
                {
                    UserId = userId,
                    Type = type,
                    Amount = amount,
                    Description = desc,
                    BalanceAfter = credit.TotalCredits,
                    ReferenceId = referenceId,
                    CreatedAt = DateTime.UtcNow
                };

                await _aiTransactionRepository.AddAsync(transaction);
                return await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task AddTransactionByEntityAsync(AITransaction transaction)
        {
            try
            {
                await _aiTransactionRepository.AddAsync(transaction);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<AITransaction> GetByUserAsync(long userId)
        {
            try
            {
                return await _aiTransactionRepository.GetByUserIdAsync(userId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdateTransactionAsync(AITransaction transaction)
        {
            try
            {
                await _aiTransactionRepository.UpdateAsync(transaction);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
