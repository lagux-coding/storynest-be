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
using StoryNest.Application.Constants;
using StoryNest.Domain.Enums;

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
        private readonly IAICreditService _aiCreditService;
        private readonly IUserMediaService _userMediaService;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAITransactionService _aiTransactionService;
        private readonly IAdminRepository _adminRepository;

        public AuthService(IUserRepository userRepository, IUnitOfWork unitOfWork, IJwtService jwtService, IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository, WelcomeEmailSender welcomeEmailSender, IRedisService redisService, IAICreditService aiCreditService, IUserMediaService userMediaService, ICurrentUserService currentUserService, IAITransactionService aiTransactionService, IAdminRepository adminRepository)
        {
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _jwtService = jwtService;
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepository;
            _welcomeEmailSender = welcomeEmailSender;
            _redisService = redisService;
            _aiCreditService = aiCreditService;
            _userMediaService = userMediaService;
            _currentUserService = currentUserService;
            _aiTransactionService = aiTransactionService;
            _adminRepository = adminRepository;
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
            var rt = new RefreshToken
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

            var activeSub = user.Subscriptions?
                        .FirstOrDefault(s => s.Status == SubscriptionStatus.Active);

            var credits = await _aiCreditService.GetUserCredit(user.Id);

            return new LoginUserResponse
            {
                UserId = user.Id,
                Username = user.Username,
                AvatarUrl = user.AvatarUrl,
                PlanName = activeSub?.Plan?.Name,
                PlanId = activeSub?.Plan?.Id,
                Credits = credits.TotalCredits,
                AccessToken = accessToken,
                RefreshToken = refresTokenPlain,
            };
        }

        public async Task<LoginAdminResponse> LoginAdminAsync(LoginAdminRequest request)
        {
            var userNameOrEmail = request.UsernameOrEmail.Trim();
            var password = request.Password.Trim();

            var admin = await _adminRepository.GetAdminByUsernameOrEmail(userNameOrEmail);
            if (admin == null || !PasswordHelper.VerifyPassword(password, admin.PasswordHash))
            {
                return null;
            }

            // Generate token
            var accessToken = _jwtService.GenerateAccessToken(admin.Id, admin.Username, admin.Email, "admin", out var jwtId);
            var refresTokenPlain = _jwtService.GenerateRefreshToken();
            var refreshTokenExpiryDays = _configuration["REFRESH_TOKEN_EXPIREDAYS"];
            var rt = new RefreshToken
            {
                AdminId = admin.Id,
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

            return new LoginAdminResponse
            {
                Id = admin.Id,
                Username = admin.Username,
                AvatarUrl = admin.AvatarUrl,
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
                FullName = request.FullName,
                PasswordHash = PasswordHelper.HashPassword(request.Password),
                AvatarUrl = GetRandomAvatar()
            };            

            await _userRepository.AddAsync(user);
            await _unitOfWork.SaveAsync();

            await _userMediaService.AddUserMedia(user.Id, user.AvatarUrl!, MediaType.Image, UserMediaStatus.Confirmed);
            await _aiCreditService.AddCreditsAsync(user.Id, 10);

            await _aiTransactionService.AddTransactionAsync(user.Id, int.Parse(user.Id.ToString()), 10, "init credits", AITransactionType.Earned);

            await _welcomeEmailSender.SendAsync(
                user.Email,
                user.Username,
                "https://storynest.io.vn",
                CancellationToken.None
            );

            return true;
        }

        public async Task<RefreshTokenResponse?> RefreshAsync(string access, string refreshToken)
        {
            // 1. Get principal from expired token
            var principal = _jwtService.GetPrincipalFromExpiredToken(access);
            if (principal == null) return null;

            var userIdStr = principal.FindFirstValue(JwtRegisteredClaimNames.Sub);
            var jwtId = principal.FindFirstValue(JwtRegisteredClaimNames.Jti);
            if (string.IsNullOrEmpty(userIdStr) || string.IsNullOrEmpty(jwtId)) return null;

            var userId = long.Parse(userIdStr);

            // 2. Get refresh token by Hash
            var refreshHash = HashHelper.SHA256(refreshToken);
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
            var newRt = new RefreshToken
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

        public async Task<bool> LogoutAsync(string refreshTokenPlain, string type)
        {
            var hash = HashHelper.SHA256(refreshTokenPlain);
            var stored = await _refreshTokenRepository.GetByHashAsync(hash);
            if (stored == null || !stored.IsActive) return false;

            stored.RevokedAt = DateTime.UtcNow;
            stored.RevokedBy = type;
            stored.RevokeReason = "logout";
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

        private string GetRandomAvatar()    
        {
            var rnd = new Random();
            int index = rnd.Next(DefaultAvatars.Avatars.Count);
            string result = DefaultAvatars.Avatars[index];
            return result;
        }
    }
}
