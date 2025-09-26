using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IPaymentService
    {
        public Task<int> AddPaymentAsync(long userId, int subscriptionId, decimal amount, string currency, string provider, string providerTXN, PaymentStatus status);
        Task<Payment> GetPaymentByTXN(string code);
        public Task<Payment> GetPaymentByUserAndSubAsync(long userId, int subscriptionId);
        public Task UpdatePaymentAsync(Payment payment);
    }
}
