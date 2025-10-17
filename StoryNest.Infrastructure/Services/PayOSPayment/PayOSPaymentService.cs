using Microsoft.AspNetCore.SignalR;
using Microsoft.Extensions.Configuration;
using Net.payOS;
using Net.payOS.Types;
using StoryNest.Application.Dtos.Dto;
using StoryNest.Application.Features.Users;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using StoryNest.Infrastructure.Services.S3;

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
        private readonly IQuestPdfService _pdfService;
        private readonly IS3Service _s3Service;
        private readonly InvoiceEmailSender _invoiceEmailSender;
        private readonly IUnitOfWork _unitOfWork;

        public PayOSPaymentService(IConfiguration configuration, ISubscriptionService subscriptionService, IPaymentService paymentService, IPlanService planService, IAITransactionService aiTransactionService, IAICreditService aiCreditService, IUnitOfWork unitOfWork, IQuestPdfService pdfService, IS3Service s3Service, InvoiceEmailSender invoiceEmailSender)
        {
            _configuration = configuration;
            _subscriptionService = subscriptionService;
            _paymentService = paymentService;
            _planService = planService;
            _aiTransactionService = aiTransactionService;
            _aiCreditService = aiCreditService;
            _unitOfWork = unitOfWork;
            _pdfService = pdfService;
            _s3Service = s3Service;
            _invoiceEmailSender = invoiceEmailSender;
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
                var sub = await _subscriptionService.GetPendingSubByUser(userId);
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

                var startDate = DateTime.UtcNow;
                var endDate = startDate.AddMonths(1);

                // Check if activated payment and subscription
                var existingSub = await _subscriptionService.GetActiveSubByUser(userId);
                if (existingSub != null)
                {
                    existingSub.StartDate = startDate;
                    existingSub.EndDate = endDate;
                    existingSub.PlanId = planId;
                    existingSub.UpdatedAt = DateTime.UtcNow;
                    existingSub.Status = SubscriptionStatus.Pending;
                    await _subscriptionService.UpdateSubscriptionAsync(existingSub);

                    // Create payment
                    await _paymentService.AddPaymentAsync(userId, existingSub.Id, plan.PriceMonthly, "VND", "PayOS", orderCode.ToString(), PaymentStatus.Pending);
                    ItemData item = new ItemData(plan.Name, 1, (int)plan.PriceMonthly);
                    List<ItemData> items = new List<ItemData>();
                    items.Add(item);
                    long expiredAt = new DateTimeOffset(DateTime.UtcNow.AddMinutes(10))
                            .ToUnixTimeSeconds();
                    PaymentData paymentData = new PaymentData(orderCode, (int)plan.PriceMonthly, "StoryNest", items, "https://dev.storynest.io.vn", "https://dev.storynest.io.vn", expiredAt: expiredAt);
                    CreatePaymentResult createPayment = await payos.createPaymentLink(paymentData);
                    return createPayment;
                }

                // Create subscription
                var check1 = await _subscriptionService.AddSubscriptionAsync(userId, planId, startDate, endDate, SubscriptionStatus.Pending);

                var sub = await _subscriptionService.GetPendingSubByUser(userId);

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

        public async Task<CreatePaymentResult> CheckoutV2Async(long userId, int planId)
        {
            // Load PayOS config
            var clientId = _configuration["PAYOS_CLIENT_ID"];
            var apiKey = _configuration["PAYOS_API_KEY"];
            var checksum = _configuration["PAYOS_CHECKSUM"];
            var domain = _configuration["FRONTEND_URL"] ?? "https://storynest.io.vn";
            var payos = new PayOS(clientId, apiKey, checksum);

            // Generate order code
            long orderCode = GenerateOrderCode();

            // Get plan
            var plan = await _planService.GetPlanByIdAsync(planId)
                ?? throw new Exception($"Plan not found: {planId}");

            var startDate = DateTime.UtcNow;
            var endDate = startDate.AddMonths(1);
            Subscription sub;
            await using var tx = await _unitOfWork.BeginTransactionAsync();
            try
            {
                sub = await _subscriptionService.GetActiveSubByUser(userId);
                if (sub != null)
                {
                    sub.PlanId = planId;
                    sub.StartDate = startDate;
                    sub.EndDate = endDate;
                    sub.Status = SubscriptionStatus.Pending;
                    sub.UpdatedAt = DateTime.UtcNow;
                    await _subscriptionService.UpdateSubscriptionAsync(sub);
                }
                else
                {
                    sub = new Subscription
                    {
                        UserId = userId,
                        PlanId = planId,
                        StartDate = startDate,
                        EndDate = endDate,
                        Status = SubscriptionStatus.Pending,
                        CreatedAt = DateTime.UtcNow
                    };
                    await _subscriptionService.AddSubscriptionAsync(sub.UserId, sub.PlanId, startDate, endDate, sub.Status);

                    sub = await _subscriptionService.GetPendingSubByUser(userId);
                }

                // Create payment record
                await _paymentService.AddPaymentAsync(
                    userId,
                    sub.Id,
                    plan.PriceMonthly,
                    "VND",
                    "PayOS",
                    orderCode.ToString(),
                    PaymentStatus.Pending
                );

                await _unitOfWork.SaveAsync();
                await tx.CommitAsync();
            }
            catch (Exception)
            {
                await tx.RollbackAsync();
                throw;
            }

            // Create PayOS payment link
            ItemData item = new ItemData(plan.Name, 1, (int)plan.PriceMonthly);
            List<ItemData> items = new List<ItemData> { item };
            var expiredAt = DateTimeOffset.UtcNow.AddMinutes(10).ToUnixTimeSeconds();
            var paymentData = new PaymentData(
                orderCode,
                (int)plan.PriceMonthly,
                "StoryNest Subscription",
                items,
                cancelUrl: $"{domain}/cancel",
                returnUrl: $"{domain}/success",
                expiredAt: expiredAt
            );

            var result = await payos.createPaymentLink(paymentData);
            return result;
        }

        private static long GenerateOrderCode()
        {
            string raw = DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString() + Random.Shared.Next(100, 999);
            return long.Parse(raw.Substring(raw.Length - 9));
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
                    payment.PaidAt = DateTime.UtcNow;
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

                    var transaction = await _aiTransactionService.AddTransactionAsync(payment.UserId, (int)payment.UserId, sub.Plan.AiCreditsDaily, $"upgrade {sub.Plan.Name} plan", AITransactionType.Earned);

                    // Send pdf through mail
                    var invoiceDto = new InvoiceDto
                    {
                        OrderCode = data.orderCode,
                        Amount = data.amount,
                        IssueDate = DateTime.UtcNow,
                        User = sub.User,
                        Subscription = sub,
                        Payment = payment
                    };

                    var pdfBytes = _pdfService.Generate(invoiceDto);

                    using var ms = new MemoryStream(pdfBytes);
                    string key = await _s3Service.UploadInvoice(ms, invoiceDto.OrderCode, sub.UserId);
                    string pdfUrl = $"{_configuration["CDN_DOMAIN"]}/{key}";

                    await _invoiceEmailSender.SendAsync(sub.User.Email, sub.User.FullName, invoiceDto.OrderCode, sub.Plan.Name, sub.StartDate, sub.EndDate, invoiceDto.Amount, invoiceDto.Payment.Currency, "VietQR", invoiceDto.Payment.PaidAt?.ToString("g"), pdfUrl, CancellationToken.None);

                    return true;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception ex)
            {
                return false;
            }
        }
    }
}
