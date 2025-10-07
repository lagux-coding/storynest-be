using AutoMapper;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;

        public UserService(IUserRepository userRepository, IMapper mapper)
        {
            _userRepository = userRepository;
            _mapper = mapper;
        }

        public async Task<List<User>> GetAllUser()
        {
            return await _userRepository.GetAllUserAsync();
        }

        public async Task<UserFullResponse> GetMe(long userId)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null) throw new Exception("User not found or not active anymore");
                var userDto = _mapper.Map<UserFullResponse>(user);
                return userDto;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        public async Task<User?> GetUserByEmailAsync(string email)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return null;
            return user;
        }

        public Task<User?> GetUserByIdAsync(long userId)
        {
            var user = _userRepository.GetByIdAsync(userId);
            if (user == null) return null;
            return user;
        }

        public async Task UpdateUserAsync(User user)
        {
            await _userRepository.UpdateAsync(user);
        }
    }
}
