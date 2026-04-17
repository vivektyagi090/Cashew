using WebApi.Models;

namespace WebApi.Repository.Interface
{
    public interface IUserRepository
    {
        Task<UserModel?> GetUserByIdAsync(int id);
        Task<UserModel?> GetUserByUsernameAsync(string username);
        Task<UserModel?> GetUserByEmailAsync(string email);
        Task<int> CreateUserAsync(UserModel user);
        Task<bool> UpdateUserAsync(UserModel user);
        Task<bool> DeleteUserAsync(int id);
        Task<bool> UserExistsAsync(string username, string email);
        Task<bool> UpdatePasswordAsync(int userId, string newPasswordHash);
        Task<bool> UpdateEmailVerificationStatusAsync(int userId, bool isVerified);
        Task<bool> ActivateUserAsync(int userId);
        Task<bool> DeactivateUserAsync(int userId);
        // Task<(IEnumerable<UserModel> Users, int TotalCount)> GetAllUsersAsync();
        Task<(IEnumerable<UserModel> Users, int TotalCount)> GetAllUsersAsync(int pageNumber = 1,int pageSize = 20, string? searchTerm = null);
        Task<UserModel?> GetByEmailVerificationTokenAsync(string token);
        Task<bool> VerifyEmailAsync(int userId);
        Task SavePasswordResetTokenAsync(int userId,string token,DateTime expiry);
        Task<UserModel?> GetUserByPasswordResetTokenAsync(string token);
        Task<bool> UpdateRefreshTokenAsync(UserModel user);
    }
}
