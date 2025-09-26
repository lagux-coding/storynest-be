using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface ISubscriptionService
    {
        public Task<int> AddSubscriptionAsync(long userId, int planId, DateTime startDate, DateTime endDate, SubscriptionStatus status);
        public Task<Subscription> GetActiveSubByUser(long userId);
        public Task<Subscription> GetByIdAsync(int subscriptionId);
        public Task<Subscription> GetPendingSubByUser(long userId);
        public Task UpdateSubscriptionAsync(Subscription sub);
    }
}
