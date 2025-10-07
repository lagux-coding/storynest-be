using StoryNest.Application.Dtos.Dto;
using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Features.Users
{
    public class InvoiceEmailSender
    {
        private readonly IEmailService _emailService;
        private readonly ITemplateRenderer _renderer;

        public InvoiceEmailSender(ITemplateRenderer renderer, IEmailService emailService)
        {
            _renderer = renderer;
            _emailService = emailService;
        }

        public async Task SendAsync(
            string toEmail,
            string displayName,
            long invoiceCode,
            string planName,
            DateTime startDate,
            DateTime endDate,
            int amount,
            string currency,
            string provider,
            string paidAt,
            string invoiceUrl,
            CancellationToken ct)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh"); // UTC+7
            var startDateLocal = TimeZoneInfo.ConvertTimeFromUtc(startDate, timeZone);
            var endDateLocal = TimeZoneInfo.ConvertTimeFromUtc(endDate, timeZone);

            var data = new Dictionary<string, string>
            {
                ["DisplayName"] = displayName,
                ["InvoiceCode"] = invoiceCode.ToString(),
                ["PlanName"] = planName,
                ["StartDate"] = startDateLocal.ToString("dd/MM/yyyy HH:mm"),
                ["EndDate"] = endDateLocal.ToString("dd/MM/yyyy HH:mm"),
                ["Amount"] = amount.ToString(),
                ["Currency"] = currency,
                ["Provider"] = provider,
                ["PaidAt"] = paidAt,
                ["InvoiceUrl"] = invoiceUrl,
                ["Year"] = DateTime.UtcNow.Year.ToString()
            };

            var html = _renderer.Render("Invoice", data);

            await _emailService.SendAsync(
                toEmail,
                $"[StoryNest] Hóa đơn cho gói đăng ký của bạn #{invoiceCode}",
                html,
                ct: ct
            );
        }
    }
}

