using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Enums;

namespace StoryNest.Infrastructure.Services.PayOSPayment
{
    public class PayOSPaymentService : IPayOSPaymentService
    {
        private readonly IConfiguration _configuration;
        private readonly IPlanService _planService;
        private readonly ISubscriptionService _subscriptionService;
        private readonly IPaymentService _paymentService;
        private readonly IAICreditService _aiCreditService;
        private readonly IAITransactionService _aiTransactionService;
        private readonly IUnitOfWork _unitOfWork;

        public PayOSPaymentService(IConfiguration configuration, ISubscriptionService subscriptionService, IPaymentService paymentService, IPlanService planService, IAITransactionService aiTransactionService, IAICreditService aiCreditService, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _subscriptionService = subscriptionService;
            _paymentService = paymentService;
            _planService = planService;
            _aiTransactionService = aiTransactionService;
            _aiCreditService = aiCreditService;
            _unitOfWork = unitOfWork;
        }

        public async Task<bool> CancelAsync(long userId, long orderCode)
        {
            try
            {
                var clientId = _configuration["PAYOS_CLIENT_ID"];
                var apiKey = _configuration["PAYOS_API_KEY"];
                var checksum = _configuration["PAYOS_CHECKSUM"];

                PayOS payos = new PayOS(clientId, apiKey, checksum);
                PaymentLinkInformation cancelledPaymentLinkInfo = await payos.cancelPaymentLink(orderCode, "User cancel");

                // Cancel payment
                var sub = await _subscriptionService.GetActiveSubByUser(userId);
                var payment = await _paymentService.GetPaymentByUserAndSubAsync(userId, sub.Id);
                payment.Status = PaymentStatus.Failed;
                await _paymentService.UpdatePaymentAsync(payment);

                // Cancel subscription
                sub.Status = SubscriptionStatus.Cancelled;
                await _subscriptionService.UpdateSubscriptionAsync(sub);

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<CreatePaymentResult> CheckoutAsync(long userId, int planId)
        {
            try
            {
                var clientId = _configuration["PAYOS_CLIENT_ID"];
                var apiKey = _configuration["PAYOS_API_KEY"];
                var checksum = _configuration["PAYOS_CHECKSUM"];

                PayOS payos = new PayOS(clientId, apiKey, checksum);

                string raw = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + Random.Shared.Next(100, 999);
                long orderCode = long.Parse(raw.Substring(raw.Length - 9));
                // Get plan
                var plan = await _planService.GetPlanByIdAsync(planId);

                // Create subscription
                var startDate = DateTime.UtcNow;
                var endDate = startDate.AddMonths(1);
                var check1 = await _subscriptionService.AddSubscriptionAsync(userId, planId, startDate, endDate, SubscriptionStatus.Pending);

                var sub = await _subscriptionService.GetActiveSubByUser(userId);

                int check2 = 0;
                if (check1 > 0)
                {
                    // Create payment
                    check2 = await _paymentService.AddPaymentAsync(userId, sub.Id, plan.PriceMonthly, "VND", "PayOS", orderCode.ToString(), PaymentStatus.Pending);
                }

                if (check2 > 0)
                {
                    ItemData item = new ItemData(plan.Name, 1, (int)plan.PriceMonthly);
                    List<ItemData> items = new List<ItemData>();
                    items.Add(item);
                    long expiredAt = new DateTimeOffset(DateTime.UtcNow.AddMinutes(10))
                            .ToUnixTimeSeconds();
                    PaymentData paymentData = new PaymentData(orderCode, (int)plan.PriceMonthly, "StoryNest", items, "https://dev.storynest.io.vn", "https://dev.storynest.io.vn", expiredAt: expiredAt);
                    CreatePaymentResult createPayment = await payos.createPaymentLink(paymentData);
                    return createPayment;
                }
                else
                {
                    throw new Exception("Failed to create payment record");
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<bool> WebhookAsync(WebhookType body)
        {
            try
            {
                var clientId = _configuration["PAYOS_CLIENT_ID"];
                var apiKey = _configuration["PAYOS_API_KEY"];
                var checksum = _configuration["PAYOS_CHECKSUM"];

                PayOS _payOS = new PayOS(clientId, apiKey, checksum);
                WebhookData data = _payOS.verifyPaymentWebhookData(body);


                if (data.code == "00")
                {
                    // Change status
                    var payment = await _paymentService.GetPaymentByTXN(data.orderCode.ToString());
                    var sub = await _subscriptionService.GetByIdAsync(payment.SubscriptionId);

                    payment.Status = PaymentStatus.Success;
                    sub.Status = SubscriptionStatus.Active;

                    await _paymentService.UpdatePaymentAsync(payment);
                    await _subscriptionService.UpdateSubscriptionAsync(sub);

                    // Update credits
                    // AI credit
                    var credit = await _aiCreditService.GetUserCredit(payment.UserId);
                    credit.TotalCredits += sub.Plan.AiCreditsDaily;
                    await _aiCreditService.UpdateCreditsAsync(credit);
                    await _unitOfWork.SaveAsync();
                    // AI transaction credit

                    var transaction = await _aiTransactionService.GetByUserAsync(payment.UserId);
                    transaction.Type = AITransactionType.Earned;
                    transaction.Amount = sub.Plan.AiCreditsDaily;
                    await _aiTransactionService.UpdateTransactionAsync(transaction);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                throw;
            }
        }
    }
}
