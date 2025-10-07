using StoryNest.Application.Dtos.Request;
using StoryNest.Application.Dtos.Response;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace StoryNest.Application.Interfaces
{
    public interface IAuthService
    {
        Task<bool> RegisterAsync(RegisterUserRequest request);
        Task<LoginUserResponse> LoginAsync(LoginUserRequest request);
        Task<bool> LogoutAsync(string refreshTokenPlain, string type);
        public Task<RefreshTokenResponse?> RefreshAsync(string access, string refreshToken);
        Task<bool> ResetPasswordAsync(ResetPasswordUserRequest request);
        Task<int> RevokeAllAsync(long userId, string? reason = null, string? revokedBy = null);
    }
}
