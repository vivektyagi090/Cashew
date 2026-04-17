using System.Security.Claims;
using WebApi.Models;

namespace WebApi.Repository.Interface
{
    public interface IJwtService
    {
        string GenerateToken(UserModel user);
        string GenerateRefreshToken();
        ClaimsPrincipal GetPrincipalFromExpiredToken(string token);
        Task<bool> SaveRefreshTokenAsync(int userId, string refreshToken, DateTime expiry);
        Task<bool> ValidateRefreshTokenAsync(int userId, string refreshToken);
        Task<bool> RevokeRefreshTokenAsync(int userId);
    }
}
