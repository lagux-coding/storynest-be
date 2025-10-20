using Microsoft.Extensions.Configuration;
using StoryNest.Domain.Entities;
using StoryNest.Shared.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Persistence.Seed
{
    public static class ApplicationDbContextSeed
    {
        public static void SeedAdmins(MyDbContext context, IConfiguration configuration)
        {
            if (!context.Admins.Any())
            {
                var admin = new Admin()
                {
                    Username = configuration["ADMIN_USERNAME"],
                    Email = configuration["ADMIN_EMAIL"],
                    FullName = "Super Admin",
                    AvatarUrl = configuration["ADMIN_AVATAR"],
                    PasswordHash = PasswordHelper.HashPassword(configuration["ADMIN_PASSWORD"]),
                    IsSuperAdmin = true,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow
                };

                context.Admins.Add(admin);
                context.SaveChanges();
            }
        }
    }
}
