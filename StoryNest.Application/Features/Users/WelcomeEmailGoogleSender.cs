using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Features.Users
{
    public class WelcomeEmailGoogleSender
    {
        private readonly IEmailService _emailService;
        private readonly ITemplateRenderer _renderer;

        public WelcomeEmailGoogleSender(ITemplateRenderer renderer, IEmailService emailService)
        {
            _renderer = renderer;
            _emailService = emailService;
        }

        public async Task SendAsync(string toEmail, string displayName, string email, string username, string password, string url, CancellationToken ct)
        {
            var data = new Dictionary<string, string>
            {
                ["DisplayName"] = displayName,
                ["Email"] = email,
                ["Username"] = username,
                ["Password"] = password,
                ["LoginUrl"] = url,
                ["Year"] = DateTime.UtcNow.Year.ToString()
            };

            var html = _renderer.Render("Welcome", data);

            await _emailService.SendAsync(toEmail, "Thông tin tài khoản StoryNest của bạn", html, ct: ct);
        }
    }
}
