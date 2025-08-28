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

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<ApiResponse<object>>> Register([FromBody] RegisterUserRequest request)
        {
            if (request.Password != request.ConfirmPassword)
            {
                return ApiResponse<object>.Fail("Password does not match");
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
