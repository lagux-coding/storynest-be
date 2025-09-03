using Microsoft.Extensions.Configuration;
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
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;

        public AuthService(IUserRepository userRepository, IUnitOfWork unitOfWork, IJwtService jwtService, IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepository;
        }

        public async Task<LoginUserResponse> LoginAsync(LoginUserRequest request)
        {
            var userNameOrEmail = request.UsernameOrEmail.Trim();
            var password = request.Password.Trim();

            var user = await _userRepository.GetByUsernameOrEmailAsync(userNameOrEmail);
            if (user == null || !PasswordHelper.VerifyPassword(password, user.PasswordHash))
            {
                return null;
            }

            // Generate token
            var accessToken = _jwtService.GenerateAccessToken(user.Id, user.Username, user.Email, "user", out var jwtId);
            var refresTokenPlain = _jwtService.GenerateRefreshToken();
            var refreshTokenExpiryDays = _configuration["REFRESH_TOKEN_EXPIREDAYS"];
            var rt = new RefreshTokens
            {
                UserId = user.Id,
                TokenHash = HashHelper.SHA256(refresTokenPlain),
                JwtId = jwtId,
                ExpiresAt = DateTime.UtcNow.AddDays(double.Parse(_configuration["REFRESH_TOKEN_EXPIREDAYS"])),
                CreatedAt = DateTime.UtcNow,
                DeviceId = request.DeviceId,
                IpAddress = request.IpAddress,
                UserAgent = request.UserAgent,
            };

            await _refreshTokenRepository.AddAsync(rt);
            await _unitOfWork.SaveAsync();

            return new LoginUserResponse
            {
                Username = user.Username,
                AccessToken = accessToken,
                RefreshToken = refresTokenPlain,
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
