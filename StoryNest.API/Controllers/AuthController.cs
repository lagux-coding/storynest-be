using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using StoryNest.Application.Interfaces;

namespace StoryNest.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;
        private readonly IValidator<RegisterUserRequest> _registerValidator;
        private readonly IValidator<LoginUserRequest> _loginValidator;

        public AuthController(IAuthService authService, IValidator<RegisterUserRequest> registerValidator, IValidator<LoginUserRequest> loginValidator)
        {
            _authService = authService;
            _registerValidator = registerValidator;
            _loginValidator = loginValidator;
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
                return Ok(ApiResponse<object>.Success(result, "Login successful"));
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<ApiResponse<object>>> Refresh([FromBody] RefreshTokenRequest request)
        {
            var access = request.AccessToken;
            var refresh = request.RefreshToken;
            var result = await _authService.RefreshAsync(request);
            if (result == null) return Unauthorized(ApiResponse<object>.Fail(new { }, "Invalid token or token expired"));

            return Ok(ApiResponse<RefreshTokenResponse>.Success(result));
        }

        [HttpPost("logout")]
        public async Task<ActionResult<ApiResponse<object>>> Logout([FromBody] LogoutRequest request)
        {
            var refreshTokenPlain = request.RefreshToken.Trim();
            bool result = await _authService.LogoutAsync(refreshTokenPlain);
            if (!result) return BadRequest(ApiResponse<object>.Fail("Invalid token or token expired"));
            return Ok(ApiResponse<object>.Success(new { }, "Logout successful"));
        }

        [HttpPost("revoke-all")]
        public async Task<ActionResult<ApiResponse<object>>> RevokeAll([FromQuery] long userId, [FromBody] RevokeAllRequest request)
        {
            var count = await _authService.RevokeAllAsync(userId, request.Reason, request.RevokedBy);

            return Ok(ApiResponse<object>.Success(new { revoke = count }));
        }
    }
}
