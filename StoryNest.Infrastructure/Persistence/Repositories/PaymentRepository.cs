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
    public class PaymentRepository : IPaymentRepository
    {
        private readonly MyDbContext _context;

        public PaymentRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task AddAsync(Payment payment)
        {
            await _context.Payments.AddAsync(payment);
        }

        public async Task<int> CountSuccessAsync()
        {
            return await _context.Payments.CountAsync(p => p.Status == PaymentStatus.Success && p.PaidAt != null);
        }

        public async Task<List<Payment>> GetAllSuccessPaymentAsync(int page = 1, int pageSize = 10)
        {
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Success && p.PaidAt != null)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .OrderByDescending(p => p.Id)
                .ToListAsync();
        }

        public async Task<Payment?> GetByTXN(string code)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.ProviderTXN == code);
        }

        public async Task<Payment?> GetByUserAndSub(long userId, int subscriptionId)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.UserId == userId && p.SubscriptionId == subscriptionId && p.Status == PaymentStatus.Pending);
        }

        public async Task<Payment?> GetSuccessByTXN(long userId, string code)
        {
            return await _context.Payments.FirstOrDefaultAsync(p => p.ProviderTXN == code && p.UserId == userId && p.Status == PaymentStatus.Success);
        }

        public async Task<decimal> TotalRevenue()
        {
            return await _context.Payments
                .Where(p => p.Status == PaymentStatus.Success)
                .SumAsync(p => p.Amount);
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await Task.CompletedTask;
        }
    }
}
