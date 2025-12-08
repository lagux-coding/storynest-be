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

        public async Task<int> CountSuccessAsync(string filter = "total")
        {
            var query = _context.Payments
                .Where(p => p.Status == PaymentStatus.Success && p.PaidAt != null && p.Amount >= 79000);

            switch (filter.ToLower())
            {
                case "weekly":
                    var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
                    query = query.Where(p => p.PaidAt >= sevenDaysAgo);
                    break;

                case "total":
                    break;

                default:
                    throw new ArgumentException("Invalid filter. Use 'total' or 'weekly'.");
            }

            return await query.CountAsync();
        }


        public async Task<List<Payment>> GetAllSuccessPaymentAsync(int page = 1, int pageSize = 10, string filter = "total")
        {
            var query = _context.Payments
                    .Where(p => p.Status == PaymentStatus.Success && p.PaidAt != null && p.Amount >= 79000);

            // Apply filter
            switch (filter.ToLower())
            {
                case "weekly":
                    var sevenDaysAgo = DateTime.UtcNow.AddDays(-7);
                    query = query.Where(p => p.PaidAt >= sevenDaysAgo);
                    break;

                case "total":
                    break;

                default:
                    throw new ArgumentException("Invalid filter type. Use 'total' or 'weekly'.");
            }

            return await query
                .OrderByDescending(p => p.Id)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
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
                .Where(p => p.Status == PaymentStatus.Success && p.Amount >= 79000)
                .SumAsync(p => p.Amount);
        }

        public async Task UpdateAsync(Payment payment)
        {
            _context.Payments.Update(payment);
            await Task.CompletedTask;
        }
    }
}
