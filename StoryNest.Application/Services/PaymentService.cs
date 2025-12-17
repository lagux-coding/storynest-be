using StoryNest.Application.Dtos.Response;
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
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _paymentRepository;
        private readonly IUnitOfWork _unitOfWork;

        public PaymentService(IPaymentRepository paymentRepository, IUnitOfWork unitOfWork)
        {
            _paymentRepository = paymentRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<int> AddPaymentAsync(long userId, int subscriptionId, decimal amount, string currency, string provider, string providerTXN, PaymentStatus status)
        {
            try
            {
                var payment = new Payment
                {
                    UserId = userId,
                    SubscriptionId = subscriptionId,
                    Amount = amount,
                    Currency = currency,
                    Provider = provider,
                    ProviderTXN = providerTXN,
                    Status = status,
                    CreatedAt = DateTime.UtcNow,
                };

                await _paymentRepository.AddAsync(payment);
                return await _unitOfWork.SaveAsync();               
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<PaginatedDefault<Payment>> GetAllSuccessPaymentAsync(int page = 1, int pageSize = 10, string filter = "total")
        {
            try
            {
                var items = await _paymentRepository.GetAllSuccessPaymentAsync(page, pageSize, filter);
                var totalCount = await _paymentRepository.CountSuccessAsync(filter);

                return new PaginatedDefault<Payment>
                {
                    Items = items,
                    TotalItems = totalCount,
                    PageSize = pageSize,
                    Page = page,
                    TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
                };
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Payment?> GetPaymentByTXN(string code)
        {
            try
            {
                return await _paymentRepository.GetByTXN(code);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Payment> GetPaymentByUserAndSubAsync(long userId, int subscriptionId)
        {
            try
            {
                return await _paymentRepository.GetByUserAndSub(userId, subscriptionId);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<Payment?> GetSuccessPaymentByTXN(long userId, string code)
        {
            try
            {
                return await _paymentRepository.GetSuccessByTXN(userId, code);
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<decimal> TotalRevenueAsync()
        {
            try
            {
                return await _paymentRepository.TotalRevenue();
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task UpdatePaymentAsync(Payment payment)
        {
            try
            {
                await _paymentRepository.UpdateAsync(payment);
                await _unitOfWork.SaveAsync();
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
