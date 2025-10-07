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
    public class SubscriptionService : ISubscriptionService
    {
        private readonly ISubscriptionRepository _subscriptionRepository;
        private readonly IUnitOfWork _unitOfWork;

        public SubscriptionService(IUnitOfWork unitOfWork, ISubscriptionRepository subscriptionRepository)
        {
            _unitOfWork = unitOfWork;
            _subscriptionRepository = subscriptionRepository;
        }

        public async Task<int> AddSubscriptionAsync(long userId, int planId, DateTime startDate, DateTime endDate, SubscriptionStatus status)
        {
            try
            {
                var subscription = new Subscription
                {
                    UserId = userId,
                    PlanId = planId,
                    StartDate = startDate,
                    EndDate = endDate,
                    Status = status,
                    CreatedAt = DateTime.UtcNow,
                };

                await _subscriptionRepository.AddAsync(subscription);
                return await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Subscription> GetActiveSubByUser(long userId)
        {
            try
            {
                return await _subscriptionRepository.GetActiveByUserId(userId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Subscription> GetByIdAsync(int subscriptionId)
        {
            try
            {
                return await _subscriptionRepository.GetByIdAsync(subscriptionId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Subscription> GetPendingSubByUser(long userId)
        {
            try
            {
                return await _subscriptionRepository.GetByUserId(userId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdateSubscriptionAsync(Subscription sub)
        {
            try
            {
                await _subscriptionRepository.UpdateAsync(sub);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
