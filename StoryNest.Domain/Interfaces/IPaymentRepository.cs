using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Interfaces
{
    public interface IPaymentRepository
    {
        public Task AddAsync(Payment payment);
        Task<Payment?> GetByTXN(string code);
        Task<Payment?> GetByUserAndSub(long userId, int subscriptionId);
        Task<Payment?> GetSuccessByTXN(long userId, string code);
        Task UpdateAsync(Payment payment);
        public Task<decimal> TotalRevenue();
    }
}
