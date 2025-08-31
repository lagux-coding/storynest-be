using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Interfaces;
using StoryNest.Shared.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IUnitOfWork _unitOfWork;

        public AuthService(IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginUserResponse> LoginAsync(LoginUserRequest request)
        {
            var user = await _userRepository.GetByUsernameOrEmailAsync(request.UsernameOrEmail);
            if (user == null || !PasswordHelper.VerifyPassword(request.Password, user.PasswordHash))
            {
                return null;
            }

            return new LoginUserResponse
            {
                Token = "This is a token",
                Expiration = DateTime.UtcNow.AddMinutes(15)
            };
        }

        public async Task<bool> RegisterAsync(RegisterUserRequest request)
        {
            if (await _userRepository.GetByUsernameOrEmailAsync(request.Email) != null ||
                await _userRepository.GetByUsernameOrEmailAsync(request.Username) != null) return false;

            var user = new User
            {
                Username = request.Username,
                Email = request.Email,
                PasswordHash = PasswordHelper.HashPassword(request.Password)
            };

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveAsync();
            return true;
        }
    }
}
