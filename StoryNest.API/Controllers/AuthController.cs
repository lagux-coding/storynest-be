using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Features.Users;
using StoryNest.Application.Interfaces;
using StoryNest.Domain.Entities;
using StoryNest.Domain.Enums;
using System.Text.Json;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly IAuthService _authService;
        private readonly IUserService _userService;
        private readonly IJwtService _jwtService;
        private readonly IGoogleService _googleService;
        private readonly ResetPasswordEmailSender _resetPasswordEmailSender;
        private readonly IValidator<RegisterUserRequest> _registerValidator;
        private readonly IValidator<LoginUserRequest> _loginValidator;
        private readonly ICurrentUserService _currentUserService;
        private readonly ILogger<AuthController> _logger;

        public AuthController(IAuthService authService, IValidator<RegisterUserRequest> registerValidator, IValidator<LoginUserRequest> loginValidator, IConfiguration configuration, IUserService userService, IJwtService jwtService, ResetPasswordEmailSender resetPasswordEmailSender, IGoogleService googleService, ICurrentUserService currentUserService, ILogger<AuthController> logger)
        {
            _authService = authService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _configuration = configuration;
            _userService = userService;
            _jwtService = jwtService;
            _resetPasswordEmailSender = resetPasswordEmailSender;
            _googleService = googleService;
            _currentUserService = currentUserService;
            _logger = logger;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<object>>> Register([FromBody] RegisterUserRequest request)
        {
            var validationResult = await _registerValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).Distinct().ToArray()
                    );
                return BadRequest(ApiResponse<object>.Fail(errors, "Validation errors"));
            }

            bool check = await _authService.RegisterAsync(request);
            if (!check)
            {
                return BadRequest(ApiResponse<object>.Fail("Email or Username already exists"));
            }
            else
            {
                return Ok(ApiResponse<object>.Success(new { }, "User registered successfully"));
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<ApiResponse<object>>> Login([FromBody] LoginUserRequest request)
        {
            var validationResult = await _loginValidator.ValidateAsync(request);
            if (!validationResult.IsValid)
            {
                var errors = validationResult.Errors
                    .GroupBy(e => e.PropertyName)
                    .ToDictionary(
                        g => g.Key,
                        g => g.Select(e => e.ErrorMessage).Distinct().ToArray()
                    );
                return BadRequest(ApiResponse<object>.Fail(errors, "Validation errors"));
            }

            var result = await _authService.LoginAsync(request);
            if (result == null)
            {
                return BadRequest(ApiResponse<object>.Fail("Invalid username or password"));
            }
            else
            {
                SetRefreshTokenCookie(Response, result.RefreshToken, DateTime.UtcNow.AddDays(double.Parse(_configuration["REFRESH_TOKEN_EXPIREDAYS"])));
                return Ok(ApiResponse<object>.Success(result, "Login successful"));
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<object>>> Refresh()
        {

            if (!Request.Cookies.TryGetValue("refreshToken", out var refresh))
            {
                return Unauthorized(ApiResponse<object>.Fail(new { }, "No refresh token found"));
            }

            //var access = request.AccessToken;
            var accessHeader = Request.Headers["Authorization"].ToString();
            var access = string.IsNullOrEmpty(accessHeader)
                            ? null
                            : accessHeader.Replace("Bearer ", "");

            var result = await _authService.RefreshAsync(access, refresh);
            if (result == null) return Unauthorized(ApiResponse<object>.Fail(new { }, "Invalid token or token expired"));

            SetRefreshTokenCookie(Response, result.RefreshToken, DateTime.UtcNow.AddDays(double.Parse(_configuration["REFRESH_TOKEN_EXPIREDAYS"])));

            return Ok(ApiResponse<RefreshTokenResponse>.Success(result));
        }

        [Authorize]
        [HttpGet("logout")]
        public async Task<ActionResult<ApiResponse<object>>> Logout()
        {
            var type = _currentUserService.Type;
            if (type == null)
            {
                return Unauthorized(ApiResponse<object>.Fail("Authentication failed"));
            }

            if (!Request.Cookies.TryGetValue("refreshToken", out var refresh))
            {
                return Unauthorized(ApiResponse<object>.Fail(new { }, "No refresh token found"));
            }

            bool result = await _authService.LogoutAsync(refresh, type);
            if (!result) return BadRequest(ApiResponse<object>.Fail("Invalid token or token expired"));

            SetRefreshTokenCookie(Response, "", DateTime.UtcNow.AddDays(-1));
            return Ok(ApiResponse<object>.Success(new { }, "Logout successful"));
        }

        [HttpPost("forgot-password")]
        public async Task<ActionResult<ApiResponse<object>>> ForgotPassword([FromBody] ForgotPasswordUserRequest request)
        {
            var user = await _userService.GetUserByEmailAsync(request.Email);
            if (user == null)
            {
                return NotFound(ApiResponse<object>.NotFound("Email not found"));
            }

            var token = await _jwtService.GenerateResetPasswordToken(user);
            var resetLink = $"{_configuration["FRONTEND_URL"]}/reset-password?tk={Uri.EscapeDataString(token)}";

            // Send email
            try
            {
                await _resetPasswordEmailSender.SendAsync(user.Email, user.Username, resetLink, CancellationToken.None);
            }
            catch (Exception ex)
            {
                return StatusCode(500, ApiResponse<object>.Error("Failed to send email: " + ex.Message));
            }

            return Ok(ApiResponse<object>.Success(new { }, "Password reset email sent"));
        }

        [HttpPost("verify-reset")]
        public async Task<ActionResult<ApiResponse<object>>> VerifyResetPasswordLink([FromQuery] string token)
        {
            var principal = await _jwtService.VerifyResetPasswordToken(token);
            if (principal == null)
                return BadRequest(ApiResponse<object>.Fail("Invalid or expired token"));

            var username = principal.FindFirst("unique_name")?.Value;
            var email = principal.FindFirst("email")?.Value;

            return Ok(ApiResponse<object>.Success(new { username, email }, "Token is valid"));
        }

        [HttpPost("reset-password")]
        public async Task<ActionResult<ApiResponse<object>>> ResetPassword([FromBody] ResetPasswordUserRequest request)
        {
            var result = await _authService.ResetPasswordAsync(request);
            if (!result) 
                return BadRequest(ApiResponse<object>.Fail("Invalid token or token expired"));

            return Ok(ApiResponse<object>.Success(new { }, "Password has been reset successfully"));
        }

        [Authorize]
        [HttpPost("revoke-all")]
        public async Task<ActionResult<ApiResponse<object>>> RevokeAll([FromQuery] long userId, [FromBody] RevokeAllRequest request)
        {   
            var count = await _authService.RevokeAllAsync(userId, request.Reason, request.RevokedBy);

            return Ok(ApiResponse<object>.Success(new { revoke = count }));
        }

        [HttpGet("google-login")]
        public IActionResult GoogleLogin()
        {
            var clientId = _configuration["GOOGLE_CLIENT_ID"];
            var redirectUri = _configuration["GOOGLE_REDIRECT_URI"];
            var scope = "openid profile email";

            var url = $"https://accounts.google.com/o/oauth2/v2/auth?response_type=code&client_id={clientId}&redirect_uri={redirectUri}&scope={scope}&access_type=offline&prompt=consent";

            return Redirect(url);
        }

        [HttpGet("google-callback")]
        public async Task<IActionResult> GoogleCallback([FromQuery] string code)
        {
            var feUrl = _configuration["FRONTEND_URL"];
            using var client = new HttpClient();
            var tokenResponse = await client.PostAsync("https://oauth2.googleapis.com/token",
                new FormUrlEncodedContent(new Dictionary<string, string>
                {
                    ["code"] = code,
                    ["client_id"] = _configuration["GOOGLE_CLIENT_ID"]!,
                    ["client_secret"] = _configuration["GOOGLE_CLIENT_SECRET"]!,
                    ["redirect_uri"] = _configuration["GOOGLE_REDIRECT_URI"]!,
                    ["grant_type"] = "authorization_code"
                }));

            if (!tokenResponse.IsSuccessStatusCode)
                return BadRequest("Failed to exchange token");

            var json = await tokenResponse.Content.ReadAsStringAsync();
            var googleToken = JsonSerializer.Deserialize<GoogleTokenResponse>(json)!;

            var result = await _googleService.GoogleLoginAsync(googleToken);
            if (result == null)
                return Redirect(feUrl);

            // set cookie refresh
            SetRefreshTokenCookie(Response, result.RefreshToken, DateTime.UtcNow.AddDays(double.Parse(_configuration["REFRESH_TOKEN_EXPIREDAYS"])));

            var queryParams = new Dictionary<string, string?>
            {
                ["token"] = result.AccessToken,
                ["avatar"] = result.AvatarUrl,
                ["userId"] = result.UserId.ToString(),
                ["planName"] = result.PlanName,
                ["planId"] = result.PlanId?.ToString(),
            };

            _logger.LogInformation(
                "Google login successful for user {Username}, redirecting to {RedirectUrl}: PlanName={PlanName}, PlanId={PlanId}, Token={Token}, Avatar={Avatar}",
                result.Username,
                feUrl,
                result.PlanName,
                result.PlanId,
                result.AccessToken,
                result.AvatarUrl
            );


            var redirectUrl = QueryHelpers.AddQueryString($"{feUrl}/google-callback", queryParams);

            // redirect về FE
            return Redirect(redirectUrl);
        }

        private void SetRefreshTokenCookie(HttpResponse response, string refreshToken, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,          
                Secure = true,            
                SameSite = SameSiteMode.None,
                MaxAge = expires - DateTime.UtcNow,
                Path = "/",
                Domain = ".storynest.io.vn"
            };

            response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

    }
}
