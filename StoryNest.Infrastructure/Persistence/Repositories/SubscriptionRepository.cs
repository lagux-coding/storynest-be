using Microsoft.EntityFrameworkCore;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Persistence.Repositories
{
    public class SubscriptionRepository : ISubscriptionRepository
    {
        private readonly MyDbContext _context;

        public SubscriptionRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Subscription subscription)
        {
            await _context.Subscriptions.AddAsync(subscription);
        }

        public async Task<Subscription?> GetActiveByUserId(long userId)
        {
            return await _context.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Active);
        }

        public async Task<Subscription?> GetByIdAsync(int subscriptionId)
        {
            return await _context.Subscriptions
                .Include(s => s.Plan)
                .FirstOrDefaultAsync(s => s.Id == subscriptionId);
        }

        public async Task<Subscription?> GetByUserId(long userId)
        {
            return await _context.Subscriptions.FirstOrDefaultAsync(s => s.UserId == userId && s.Status == SubscriptionStatus.Pending);
        }

        public async Task UpdateAsync(Subscription sub)
        {
            _context.Subscriptions.Update(sub);
            await Task.CompletedTask;
        }
    }
}
