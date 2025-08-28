using StoryNest.Application.Interfaces;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Persistence
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly MyDbContext _context;
        public IUserRepository UserRepository { get; }

        public UnitOfWork(MyDbContext context, IUserRepository userRepository)
        {
            _context = context;
            UserRepository = userRepository;
        }

        public async Task SaveAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
