using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Features.Users
{
    public class WelcomeEmailSender
    {
        private readonly IEmailService _emailService;
        private readonly ITemplateRenderer _renderer;

        public WelcomeEmailSender(ITemplateRenderer renderer, IEmailService emailService)
        {
            _renderer = renderer;
            _emailService = emailService;
        }

        public async Task SendAsync(string toEmail, string displayName, string startWritingUrl, CancellationToken ct)
        {
            var data = new Dictionary<string, string>
            {
                ["DisplayName"] = displayName,
                ["StartWritingUrl"] = startWritingUrl,
                ["Year"] = DateTime.UtcNow.Year.ToString()
            };

            var html = _renderer.Render("Welcome", data);

            await _emailService.SendAsync(toEmail, "Chào mừng gia nhập StoryNest!", html, ct: ct);
        }
    }
}
