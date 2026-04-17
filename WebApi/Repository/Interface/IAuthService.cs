using WebApi.Models;

namespace WebApi.Repository.Interface
{
    public interface IAuthService
    {
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);
        Task<LoginResponse> LoginAsync(LoginRequest request);
        Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request);
        Task<bool> LogoutAsync(int userId);
        Task<bool> CheckUsernameAvailabilityAsync(string username);
        Task<bool> CheckEmailAvailabilityAsync(string email);
        Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request);
        Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request);
        Task<bool> VerifyEmailAsync(EmailVerificationRequest request);
        Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
        Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    }
}
