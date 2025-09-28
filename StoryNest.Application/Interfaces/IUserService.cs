using StoryNest.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IUserService
    {
        Task<User> GetUserByEmailAsync(string email);
        Task<User?> GetUserByIdAsync(long userId);
        Task<List<User>> GetAllUser();
        Task UpdateUserAsync(User user);
    }
}
