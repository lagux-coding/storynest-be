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
        private readonly IRedisService _redisService;

        public JwtService(IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository, IUserRepository userRepository, IUnitOfWork unitOfWork, IRedisService redisService)
        {
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepository;
            _userRepository = userRepository;
            _unitOfWork = unitOfWork;
            _redisService = redisService;
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

        public async Task<string> GenerateResetPasswordToken(User user)
        {
            var claims = new List<Claim>
            {
                new Claim(JwtRegisteredClaimNames.Sub, user.Id.ToString()),
                new Claim(JwtRegisteredClaimNames.UniqueName, user.Username),
                new Claim(JwtRegisteredClaimNames.Email, user.Email),
                new Claim("type", "reset_password"),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["JWT_RESET_TOKEN"]));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                    issuer: _configuration["JWT_ISSUER"],
                    audience: _configuration["JWT_AUDIENCE"],
                    claims: claims,
                    notBefore: DateTime.UtcNow,
                    expires: DateTime.UtcNow.AddMinutes(double.Parse(_configuration["JWT_RESET_EXPIREMINUTES"])),
                    signingCredentials: creds
                );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public ClaimsPrincipal? GetPrincipalFromExpiredToken(string token)
        {
            if (string.IsNullOrWhiteSpace(token)) return null;
            var tokenHandler = new JwtSecurityTokenHandler();

            JwtSecurityToken? readToken;
            try
            {
                readToken = tokenHandler.ReadJwtToken(token);
            }
            catch
            {
                return null; // token malformed
            }

            if (!string.Equals(readToken.Header.Alg, SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
                return null;

            var key = _configuration["JWT_KEY"]
                      ?? throw new InvalidOperationException("JWT_KEY is missing.");
            var issuer = _configuration["JWT_ISSUER"];
            var audience = _configuration["JWT_AUDIENCE"];

            var parameters = new TokenValidationParameters
            {
                ValidateIssuerSigningKey = true,
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),

                ValidateIssuer = !string.IsNullOrEmpty(issuer),
                ValidIssuer = issuer,

                ValidateAudience = !string.IsNullOrEmpty(audience),
                ValidAudience = audience,

                ValidateLifetime = false,
                ClockSkew = TimeSpan.Zero
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, parameters, out var validatedToken);

                if (validatedToken is not JwtSecurityToken jwt
                    || !string.Equals(jwt.Header.Alg, SecurityAlgorithms.HmacSha256, StringComparison.Ordinal))
                    return null;

                return principal;
            }
            catch
            {
                return null; // signature/iss/aud/key fail
            }
        }

        public async Task<ClaimsPrincipal?> VerifyResetPasswordToken(string token)
        {
            var tokenHandler = new JwtSecurityTokenHandler();
            var key = Encoding.UTF8.GetBytes(_configuration["JWT_RESET_TOKEN"]);

            var validationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,

                ValidIssuer = _configuration["JWT_ISSUER"],
                ValidAudience = _configuration["JWT_AUDIENCE"],
                IssuerSigningKey = new SymmetricSecurityKey(key),
            };

            try
            {
                var principal = tokenHandler.ValidateToken(token, validationParameters, out var validatedToken);
                var checkBlacklist = await _redisService.IsBlacklistedAsync(principal.FindFirst(JwtRegisteredClaimNames.Jti)?.Value);
                if (checkBlacklist) return null;

                if (validatedToken is not JwtSecurityToken jwtToken ||
                    !jwtToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                var typeClaim = principal.FindFirst("type")?.Value;
                if (typeClaim != "reset_password")
                {
                    return null;
                }

                return principal;
            }
            catch (Exception e)
            {
                return null;
            }
        }
    }
}
