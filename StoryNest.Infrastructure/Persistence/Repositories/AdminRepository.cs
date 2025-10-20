using Microsoft.EntityFrameworkCore;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Persistence.Repositories
{
    public class AdminRepository : IAdminRepository
    {
        private readonly MyDbContext _context;

        public AdminRepository(MyDbContext context)
        {
            _context = context;
        }

        public async Task<Admin?> GetAdminByUsernameOrEmail(string usernameOrEmail)
        {
            return await _context.Admins
                .Include(a => a.ReportsHandled)
                .Include(a => a.RefreshTokens)
                .FirstOrDefaultAsync(a => (a.Username == usernameOrEmail || a.Email == usernameOrEmail) && a.IsActive);
        }
    }
}
