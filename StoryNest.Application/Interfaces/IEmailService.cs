using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IEmailService
    {
        Task SendAsync(string to, string subject, string htmlBody, string? textBody = null, CancellationToken ct = default);
    }
}
