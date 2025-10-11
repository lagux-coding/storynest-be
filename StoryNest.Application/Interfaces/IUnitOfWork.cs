using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface ITransaction : IAsyncDisposable
    {
        Task CommitAsync(CancellationToken cancellationToken = default);
        Task RollbackAsync(CancellationToken cancellationToken = default);
    }

    public interface IUnitOfWork
    {
        IUserRepository UserRepository { get; }
        Task<ITransaction> BeginTransactionAsync(CancellationToken cancellationToken = default);
        Task<int> SaveAsync();
    }
}
