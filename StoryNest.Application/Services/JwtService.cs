using Microsoft.IdentityModel.Tokens;
using Microsoft.Extensions.Configuration;
using StoryNest.Application.Interfaces;
using System;
using System.Collections.Generic;
using System.IdentityModel.Tokens.Jwt;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;
using System.Security.Cryptography;
using StoryNest.Domain.Entities;
using StoryNest.Application.Dtos.Request;
using StoryNest.Shared.Common.Utils;
using StoryNest.Domain.Interfaces;
using StoryNest.Application.Dtos.Response;

namespace StoryNest.Application.Services
{
    public class JwtService : IJwtService
    {
        private readonly IConfiguration _configuration;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly IUserRepository _userRepository;

        public JwtService(IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository, IUserRepository userRepository, IUnitOfWork unitOfWork)
        {
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
        }

        public string GenerateAccessToken(long userId, string username, string email, string type, out string jwtId)
        {
            jwtId = Guid.NewGuid().ToString();

            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, username),
                new Claim(JwtRegisteredClaimNames.Email, email),
                new Claim("type", type),
                new Claim(JwtRegisteredClaimNames.Jti, jwtId)
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT_KEY"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var expireMinitures = int.Parse(_configuration["JWT_EXPIREMINUTES"]);

            var token = new JwtSecurityToken(
                    issuer: _configuration["JWT_ISSUER"],
                    audience: _configuration["JWT_AUDIENCE"],
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(expireMinitures),
                    signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string GenerateRefreshToken()
        {
            var bytes = RandomNumberGenerator.GetBytes(32);
            return Convert.ToBase64String(bytes);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            throw new NotImplementedException();
        }

        public async Task<RefreshTokenResponse?> RefreshAsync(RefreshTokenRequest request)
        {
            // 1. Get principal from expired token
            var principal = GetPrincipalFromExpiredToken(request.AccessToken);
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

            var newAccessToken = GenerateAccessToken(user.Id, user.Username, user.Email, "user", out var newJwtId);
            var newRefreshTokenPlain = GenerateRefreshToken();
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
    }
}
