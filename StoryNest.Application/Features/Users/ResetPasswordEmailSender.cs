using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Features.Users
{
    public class ResetPasswordEmailSender
    {
        private readonly IEmailService _emailService;
        private readonly ITemplateRenderer _renderer;
        private readonly IConfiguration _configuration;
        private readonly ICurrentUserService _currentUserService;

        public ResetPasswordEmailSender(IEmailService emailService, ITemplateRenderer renderer, IConfiguration configuration, ICurrentUserService currentUserService)
        {
            _emailService = emailService;
            _renderer = renderer;
            _configuration = configuration;
            _currentUserService = currentUserService;
        }

        public async Task SendAsync(string toEmail, string displayName, string resetPasswordUrl, CancellationToken ct)
        {
            var ip = _currentUserService.IpAddress;
            var data = new Dictionary<string, string>
            {
                ["DisplayName"] = displayName,
                ["TokenExpiryMinutes"] = _configuration["JWT_RESET_EXPIREMINUTES"],
                ["ResetUrl"] = resetPasswordUrl,
                ["SupportEmail"] = _configuration["SUPPORT_EMAIL"],
                ["RequestIP"] = ip,
                ["Year"] = DateTime.UtcNow.Year.ToString()
            };
            var html = _renderer.Render("ResetPassword", data);
            await _emailService.SendAsync(toEmail, "Đặt lại mật khẩu tài khoản StoryNest", html, ct: ct);
        }
    }
}
