using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Interfaces
{
    public interface ISubscriptionRepository
    {
        public Task AddAsync(Subscription subscription);
        Task<Subscription?> GetActiveByUserId(long userId);
        Task<Subscription?> GetByIdAsync(int subscriptionId);
        Task<Subscription?> GetByUserId(long userId);
        Task UpdateAsync(Subscription sub);
    }
}
