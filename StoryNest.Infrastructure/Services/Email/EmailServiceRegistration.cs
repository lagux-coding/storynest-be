using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Services.Email
{
    public static class EmailServiceRegistration
    {
        public static IServiceCollection AddEmail(this IServiceCollection services, IConfiguration configuration)
        {
            var settings = new EmailSettings();
            configuration.GetSection("Email").Bind(settings);

            services.AddSingleton(settings);
            services.AddTransient<IEmailService, EmailService>();
            return services;
        }
    }
}
