using FluentValidation;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using StoryNest.API.ApiWrapper;
using StoryNest.Application.Dtos.Request;
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
                return ApiResponse<object>.Fail(errors, "Validation errors");
            }

            bool check = await _authService.RegisterAsync(request);
            if (!check)
            {
                return ApiResponse<object>.Fail("Email or Username already exists");
            }
            else
            {
                return ApiResponse<object>.Success(new { }, "User registered successfully");
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
                return ApiResponse<object>.Fail(errors, "Validation errors");
            }

            var result = await _authService.LoginAsync(request);
            if (result == null)
            {
                return ApiResponse<object>.Fail("Invalid username or password");
            }
            else
            {
                return ApiResponse<object>.Success(result, "Login successful");
            }
        }
    }
}
