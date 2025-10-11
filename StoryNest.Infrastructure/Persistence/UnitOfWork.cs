using Microsoft.EntityFrameworkCore.Storage;
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

        public async Task<int> SaveAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default)
        {
            var dbTransaction = await _context.Database.BeginTransactionAsync(cancellationToken);
            return new EfTransaction(dbTransaction);
        }

        private class EfTransaction : ITransaction
        {
            private readonly IDbContextTransaction _transaction;

            public EfTransaction(IDbContextTransaction transaction)
            {
                _transaction = transaction;
            }

            public async Task CommitAsync(CancellationToken cancellationToken = default)
            {
                await _transaction.CommitAsync(cancellationToken);
            }

            public async Task RollbackAsync(CancellationToken cancellationToken = default)
            {
                await _transaction.RollbackAsync(cancellationToken);
            }

            public async ValueTask DisposeAsync()
            {
                await _transaction.DisposeAsync();
            }
        }
    }
}
