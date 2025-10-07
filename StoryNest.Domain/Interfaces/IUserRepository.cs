using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByEmailAsync(string email);
        Task<User?> GetByIdAsync(long userId);
        Task<User?> GetByUsernameOrEmailAsync(string usernameOrEmail);
        Task UpdateAsync(User user);
        Task AddAsync(User user);
        Task<List<User>> GetAllUserAsync();
    }
}
