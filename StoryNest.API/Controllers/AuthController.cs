using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Identity.Data;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Features.Users;
using StoryNest.Application.Interfaces;

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
        private readonly ResetPasswordEmailSender _resetPasswordEmailSender;
        private readonly IValidator<RegisterUserRequest> _registerValidator;
        private readonly IValidator<LoginUserRequest> _loginValidator;

        public AuthController(IAuthService authService, IValidator<RegisterUserRequest> registerValidator, IValidator<LoginUserRequest> loginValidator, IConfiguration configuration, IUserService userService, IJwtService jwtService, ResetPasswordEmailSender resetPasswordEmailSender)
        {
            _authService = authService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
            _configuration = configuration;
            _userService = userService;
            _jwtService = jwtService;
            _resetPasswordEmailSender = resetPasswordEmailSender;
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
            if (!Request.Cookies.TryGetValue("refreshToken", out var refresh))
            {
                return Unauthorized(ApiResponse<object>.Fail(new { }, "No refresh token found"));
            }

            bool result = await _authService.LogoutAsync(refresh);
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

        private void SetRefreshTokenCookie(HttpResponse response, string refreshToken, DateTime expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,          
                Secure = true,            
                SameSite = SameSiteMode.None,
                MaxAge = expires - DateTime.UtcNow,
                Path = "/",
                Domain = ".kusl.io.vn"
            };

            response.Cookies.Append("refreshToken", refreshToken, cookieOptions);
        }

    }
}
