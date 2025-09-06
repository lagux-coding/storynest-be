using Microsoft.Extensions.Configuration;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Interfaces;
using StoryNest.Shared.Common.Utils;
using System.IdentityModel.Tokens.Jwt;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Security.Claims;
using StoryNest.Application.Features.Users;

namespace StoryNest.Application.Services
{
    public class AuthService : IAuthService
    {
        private readonly IConfiguration _configuration;
        private readonly IUserRepository _userRepository;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IJwtService _jwtService;
        private readonly WelcomeEmailSender _welcomeEmailSender;
        private readonly IRedisService _redisService;

        public AuthService(IUserRepository userRepository, IUnitOfWork unitOfWork, IJwtService jwtService, IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository, WelcomeEmailSender welcomeEmailSender, IRedisService redisService)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepository;
            _welcomeEmailSender = welcomeEmailSender;
            _redisService = redisService;
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

            await _welcomeEmailSender.SendAsync(
                user.Email,
                user.Username,
                "https://kusl.io.vn",
                CancellationToken.None
            );

            return true;
        }

        public async Task<RefreshTokenResponse?> RefreshAsync(RefreshTokenRequest request)
        {
            // 1. Get principal from expired token
            var principal = _jwtService.GetPrincipalFromExpiredToken(request.AccessToken);
            if (principal == null) return null;

            var userIdStr = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var jwtId = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);
            if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(jwtId)) return null;

            var userId = long.Parse(userIdStr);

            // 2. Get refresh token by Hash
            var refreshHash = HashHelper.SHA256(request.RefreshToken);
            var stored = await _refreshTokenRepository.GetByHashAsync(refreshHash);
            if (stored == null) return null;

            // 3. Validate refresh token
            if (stored.UserId != userId) return null;
            if (!stored.IsActive) return null;
            if (!string.Equals(stored.JwtId, jwtId, StringComparison.Ordinal)) return null;

            // 3.1 (Optional) Check device here!

            // 4. Rotate: release new new rt & revoke old rt
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null) return null;

            var newAccessToken = _jwtService.GenerateAccessToken(user.Id, user.Username, user.Email, "user", out var newJwtId);
            var newRefreshTokenPlain = _jwtService.GenerateRefreshToken();
            var newRt = new RefreshTokens
            {
                UserId = user.Id,
                TokenHash = HashHelper.SHA256(newRefreshTokenPlain),
                JwtId = newJwtId,
                ExpiresAt = DateTime.UtcNow.AddDays(double.Parse(_configuration["REFRESH_TOKEN_EXPIREDAYS"])),
                CreatedAt = DateTime.UtcNow,
                DeviceId = stored.DeviceId,
                IpAddress = stored.IpAddress,
                UserAgent = stored.UserAgent,
            };

            // 4.1 Revoke old rt
            stored.RevokedAt = DateTime.UtcNow;
            stored.ReplacedByTokenHash = newRt.TokenHash;

            await _refreshTokenRepository.AddAsync(newRt);
            await _refreshTokenRepository.UpdateAsync(stored);
            await _unitOfWork.SaveAsync();

            return new RefreshTokenResponse
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshTokenPlain
            };
        }

        public async Task<bool> LogoutAsync(string refreshTokenPlain)
        {
            var hash = HashHelper.SHA256(refreshTokenPlain);
            var stored = await _refreshTokenRepository.GetByHashAsync(hash);
            if (stored == null || !stored.IsActive) return false;

            stored.RevokedAt = DateTime.UtcNow;
            await _refreshTokenRepository.UpdateAsync(stored);
            await _unitOfWork.SaveAsync();

            return true;
        }

        public Task<int> RevokeAllAsync(long userId, string? reason = null, string? revokedBy = null)
            => _refreshTokenRepository.RevokeAllAsync(userId, revokedBy, reason);

        public async Task<bool> ResetPasswordAsync(ResetPasswordUserRequest request)
        {
            var token = request.Token;
            var newPassword = request.NewPassword.Trim();

            // Verify token
            var principall = await _jwtService.VerifyResetPasswordToken(token);
            if (principall == null)
                return false;

            // Find user
            var username = principall.FindFirst("unique_name")?.Value;
            var email = principall.FindFirst("email")?.Value;
            var user = await _userRepository.GetByUsernameOrEmailAsync(username ?? email);
            if (user == null) 
                return false;

            // Update password
            user.PasswordHash = PasswordHelper.HashPassword(newPassword);
            await _userRepository.UpdateAsync(user);
            await _unitOfWork.SaveAsync();

            // Add to blacklist
            await _redisService.SetBlacklistAsync(principall.FindFirst(JwtRegisteredClaimNames.Jti)?.Value, TimeSpan.FromMinutes(15));

            // Revoke all rt
            await RevokeAllAsync(user.Id, "password reset", "system");

            return true;
        }
    }
}
