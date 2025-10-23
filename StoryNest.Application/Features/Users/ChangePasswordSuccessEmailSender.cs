using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Features.Users
{
    public class ChangePasswordSuccessEmailSender
    {
        private readonly IEmailService _emailService;
        private readonly ITemplateRenderer _render;

        public ChangePasswordSuccessEmailSender(IEmailService emailService, ITemplateRenderer render)
        {
            _emailService = emailService;
            _render = render;
        }

        public async Task SendAsync(string toEmail, string displayName, string username, string resetUrl, CancellationToken ct)
        {
            var timeZone = TimeZoneInfo.FindSystemTimeZoneById("Asia/Ho_Chi_Minh");
            var changedAt = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, timeZone);
            var data = new Dictionary<string, string>
            {
                ["DisplayName"] = displayName,
                ["ChangedAt"] = changedAt.ToString("dd/MM/yyyy HH:mm"),
                ["Email"] = toEmail,
                ["Username"] = username,
                ["ResetUrl"] = resetUrl
            };

            var html = _render.Render("ChangePasswordSuccess", data);
            await _emailService.SendAsync(toEmail, $"[Đổi mật khẩu] Thành công đổi mật khẩu cho tài khoản StoryNest", html, ct: ct);
        }
    }
}
