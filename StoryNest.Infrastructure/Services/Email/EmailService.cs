using Resend;
using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Services.Email
{
    public sealed class EmailSettings
    {
        public string FromName { get; set; } = "StoryNest";
        public string FromEmail { get; set; } = "noreply@support.kusl.io.vn";
    }

    public class EmailService : IEmailService
    {
        private readonly IResend _resend;
        private readonly EmailSettings _settings;

        public EmailService(IResend resend, EmailSettings settings)
        {
            _resend = resend;
            _settings = settings;
        }

        public async Task SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken ct = default)
        {
            var from = $"{_settings.FromName} <{_settings.FromEmail}>";
            var request = new EmailMessage
            {
                From = from,
                To = to,
                Subject = subject,
                HtmlBody = htmlBody,
                TextBody = textBody
            };

            var result = await _resend.EmailSendAsync(request, ct);
        }
    }
}
