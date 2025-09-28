using AngleSharp.Io;
using Google.Apis.Auth;
using Microsoft.Extensions.Configuration;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Features.Users;
using StoryNest.Application.Interfaces;
using StoryNest.Application.Services;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using StoryNest.Domain.Interfaces;
using StoryNest.Shared.Common.Utils;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Infrastructure.Services.Google
{
    public class GoogleService : IGoogleService
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;
        private readonly IJwtService _jwtService;
        private readonly IUnitOfWork _unitOfWork;
        private readonly IConfiguration _configuration;
        private readonly IRefreshTokenRepository _refreshTokenRepository;
        private readonly WelcomeEmailGoogleSender _welcomeEmailGoogleSender;

        public GoogleService(IUserService userService, IAuthService authService, WelcomeEmailGoogleSender welcomeEmailGoogleSender, IJwtService jwtService, IConfiguration configuration, IRefreshTokenRepository refreshTokenRepository, IUnitOfWork unitOfWork)
        {
            _userService = userService;
            _authService = authService;
            _welcomeEmailGoogleSender = welcomeEmailGoogleSender;
            _jwtService = jwtService;
            _configuration = configuration;
            _refreshTokenRepository = refreshTokenRepository;
            _unitOfWork = unitOfWork;
        }

        public async Task<LoginUserResponse> GoogleLoginAsync(GoogleTokenResponse googleToken)
        {
            try
            {
                var payload = await GoogleJsonWebSignature.ValidateAsync(googleToken.IdToken);
                var googleUser = new
                {
                    Id = payload.Subject,
                    Email = payload.Email,
                    Name = payload.Name,
                    Picture = payload.Picture
                };

                var user = await _userService.GetUserByEmailAsync(googleUser.Email);
                if (user == null)
                {
                    var username = UsernameGenerateHelperHelper.GenerateUsername(googleUser.Name);
                    var password = PasswordHelper.GenerateBasicPassword(6);
                    var request = new RegisterUserRequest
                    {
                        Username = username,
                        Email = googleUser.Email,
                        FullName = googleUser.Name,
                        Password = password,
                        ConfirmPassword = password,
                    };

                    var check = await _authService.RegisterAsync(request);

                    if (check)
                    {
                        await _welcomeEmailGoogleSender.SendAsync(request.Email, request.Username, request.Email, request.Username, password, "https://dev.storynest.io.vn", CancellationToken.None);
                    }

                    user = await _userService.GetUserByEmailAsync(googleUser.Email);
                }

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
                };

                await _refreshTokenRepository.AddAsync(rt);
                await _unitOfWork.SaveAsync();

                var activeSub = user.Subscriptions?
                            .FirstOrDefault(s => s.Status == SubscriptionStatus.Active);

                return new LoginUserResponse
                {
                    Username = user.Username,
                    AvatarUrl = user.AvatarUrl,
                    PlanName = activeSub?.Plan?.Name,
                    PlanId = activeSub?.Plan?.Id,
                    AccessToken = accessToken,
                    RefreshToken = refresTokenPlain,
                };
            }
            catch (Exception ex)
            {
                throw new Exception(ex.Message);
            }
        }
    }
}
