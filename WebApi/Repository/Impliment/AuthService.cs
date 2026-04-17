using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;
using System.Text;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Repository.Impliment
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtService _jwtService;
        private readonly ILogger<AuthService> _logger;
        private readonly IConfiguration _configuration;
        public AuthService(IUserRepository userRepository, ILogger<AuthService> logger, IConfiguration configuration, IJwtService jwtService)
        {
            _userRepository = userRepository;
            _logger = logger;
            _configuration = configuration;
            _jwtService = jwtService;
        }


        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            try
            {

                // Validate request
                if (request.Password != request.ConfirmPassword)
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "Passwords do not match"
                    };
                }

                // Check if user already exists
                var userExists = await _userRepository.UserExistsAsync(request.Username ?? "", request.Email ?? "");
                if (userExists)
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "Username or email already exists"
                    };
                }


                // Validate request
                if (request.Password != request.ConfirmPassword)
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "Passwords do not match"
                    };
                }
                // Validate password strength
                if (!IsPasswordStrong(request.Password ?? ""))
                {
                    return new RegisterResponse
                    {
                        Success = false,
                        Message = "Password must contain at least 8 characters, including uppercase, lowercase, numbers, and special characters"
                    };
                }

                // Generate refresh token
                var refreshToken = _jwtService.GenerateRefreshToken();
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(
                    Convert.ToInt32(_configuration["Jwt:RefreshTokenExpiryInDays"])
                );


                // Create user object
                var user = new UserModel
                {
                    Username = (request.Username ?? "").Trim(),
                    Email = (request.Email ?? "").Trim().ToLower(),
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password ?? ""),
                    FullName = request.FullName?.Trim(),
                    PhoneNumber = request.PhoneNumber?.Trim(),
                    CreatedAt = DateTime.UtcNow,
                    IsActive = true,
                    IsEmailVerified = false, // Email verification can be added later
                    RefreshToken = refreshToken,
                    RefreshTokenExpiry = refreshTokenExpiry
                };

                // Save user to database
                var userId = await _userRepository.CreateUserAsync(user);

                if (userId > 0)
                {

                    var token = _jwtService.GenerateToken(user);
                    var tokenExpiry = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:TokenExpiryInMinutes"]));
                    _logger.LogInformation("User registered successfully: {Username} (ID: {UserId})",
                        user.Username, userId);

                    // Send welcome email (optional - implement separately)
                    // await SendWelcomeEmailAsync(user.Email, user.Username);

                    return new RegisterResponse
                    {
                        Success = true,
                        Message = "Registration successful",
                        Token = token,
                        RefreshToken = refreshToken,
                        UserId = userId,
                        Username = user.Username,
                        Email = user.Email,
                        CreatedAt = user.CreatedAt,
                        TokenExpiry = tokenExpiry
                    };
                }

                return new RegisterResponse
                {
                    Success = false,
                    Message = "Registration failed. Please try again."
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Registration error for user: {Username}", request.Username ?? "Unknown");

                // Return generic error message for security
                return new RegisterResponse
                {
                    Success = false,
                    Message = "An error occurred during registration. Please try again later."
                };
            }
        }


        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            try
            {

                // 1️⃣ Normalize input (CRITICAL)
                var identifier = request.UsernameOrEmail?.Trim();
                // var inputPassword = request.Password?.Trim();
                var inputPassword = request.Password;
                //var inputPassword = request.Password ?.Trim() .Normalize(NormalizationForm.FormC);

                if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(inputPassword))
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid username/email or password"
                    };
                }

                UserModel? user = null;
                if ((request.UsernameOrEmail ?? "").Contains("@"))
                {
                    user = await _userRepository.GetUserByEmailAsync(request.UsernameOrEmail ?? "");
                }
                else
                {
                    user = await _userRepository.GetUserByUsernameAsync(request.UsernameOrEmail ?? "");
                }
                if (user == null)
                {
                    _logger.LogWarning("Login failed: User not found-{Identifier}", request.UsernameOrEmail ?? "Unknown");
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid username/email or password"
                    };
                }

                // 2️⃣ Fetch user
                //UserModel user = identifier.Contains("@")
                //    ? await _userRepository.GetUserByEmailAsync(identifier)
                //    : await _userRepository.GetUserByUsernameAsync(identifier);

                //if (user == null)
                //{
                //    _logger.LogWarning("Login failed: User not found - {Identifier}", identifier);
                //    return new LoginResponse
                //    {
                //        Success = false,
                //        Message = "Invalid username/email or password"
                //    };
                //}

                //if (!user.IsActive)
                //{
                //    _logger.LogWarning("Login failed: Inactive user-{Identifier}", request.UsernameOrEmail);
                //    return new LoginResponse
                //    {
                //        Success = false,
                //        Message = "User account is inactive. Please contact support."
                //    };
                //}

                // 3️⃣ Check active
                if (!user.IsActive)
                {
                    _logger.LogWarning("Login failed: Inactive user - {Identifier}", identifier);
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "User account is inactive. Please contact support."
                    };
                }

                // 4️⃣ Validate stored password hash format
                if (string.IsNullOrWhiteSpace(user.PasswordHash) ||
                    !user.PasswordHash.StartsWith("$2"))
                {
                    _logger.LogError(
                        "Login failed: Invalid password hash format for user {Username}",
                        user.Username
                    );

                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Account password is invalid. Please reset your password."
                    };
                }

                // 5️⃣ Verify password (FIXED)
                bool passwordValid;
                try
                {

                    passwordValid = BCrypt.Net.BCrypt.Verify(inputPassword ?? "", user.PasswordHash ?? "");
                    
                   
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex,
                        "BCrypt verification failed for user {Username}", user.Username);

                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid username/email or password"
                    };
                }

                if (!passwordValid)
                {
                    _logger.LogWarning(
                        "Login failed: Incorrect password - {Username}",
                        user.Username
                    );

                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid username/email or password"
                    };
                }


                //if (!BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
                //{
                //    _logger.LogWarning("Login failed: Incorrect password-{Identifier}", user.Username);
                //    return new LoginResponse
                //    {
                //        Success = false,
                //        Message = "Invalid username/email or password"
                //    };
                //}
                var refreshToken = _jwtService.GenerateRefreshToken();
                var refreshTokenExpiry = DateTime.UtcNow.AddDays(
                    Convert.ToInt32(_configuration["Jwt:RefreshTokenExpiryInDays"])
                );
                user.RefreshToken = refreshToken;
                user.RefreshTokenExpiry = refreshTokenExpiry;
                await _userRepository.UpdateRefreshTokenAsync(user);

                var token = _jwtService.GenerateToken(user);
                var tokenExpiry = DateTime.UtcNow.AddMinutes(
                    Convert.ToDouble(_configuration["Jwt:TokenExpiryInMinutes"])
                );
                _logger.LogInformation("User logged in successfully: {Username} (ID: {UserId})",
                    user.Username, user.Id);
                return new LoginResponse
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    RefreshToken = refreshToken,
                    TokenExpiry = tokenExpiry,
                    RefreshTokenExpiry = refreshTokenExpiry,
                    User = new UserProfile
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        IsEmailVerified = user.IsEmailVerified,
                        IsActive = user.IsActive,
                        Role = user.Role,
                        CreatedAt = user.CreatedAt
                    }
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login error for user: {Identifier}", request.UsernameOrEmail ?? "Unknown");

                return new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during login. Please try again later."
                };

            }
        }
        //public async Task<LoginResponse> LoginAsync(LoginRequest request)
        //{
        //    try
        //    {
        //        // 1️⃣ Normalize input
        //        var identifier = request.UsernameOrEmail?.Trim();
        //        var inputPassword = request.Password?.Trim(); // Removing .Trim() is safer for passwords

        //        if (string.IsNullOrWhiteSpace(identifier) || string.IsNullOrWhiteSpace(inputPassword))
        //        {
        //            return new LoginResponse { Success = false, Message = "Invalid username/email or password" };
        //        }

        //        // 2️⃣ Fetch user
        //        UserModel user = identifier.Contains("@")
        //            ? await _userRepository.GetUserByEmailAsync(identifier)
        //            : await _userRepository.GetUserByUsernameAsync(identifier);

        //        if (user == null)
        //        {
        //            _logger.LogWarning("Login failed: User not found - {Identifier}", identifier);
        //            return new LoginResponse { Success = false, Message = "Invalid username/email or password" };
        //        }

        //        // 3️⃣ Check if account is active
        //        if (!user.IsActive)
        //        {
        //            _logger.LogWarning("Login failed: Inactive user - {Identifier}", identifier);
        //            return new LoginResponse { Success = false, Message = "User account is inactive. Please contact support." };
        //        }

        //        // 4️⃣ Validate stored password hash format
        //        if (string.IsNullOrWhiteSpace(user.PasswordHash) || !user.PasswordHash.StartsWith("$2"))
        //        {
        //            _logger.LogError("Login failed: Invalid hash format for {Username}", user.Username);
        //            return new LoginResponse { Success = false, Message = "Account password format is invalid." };
        //        }

        //        // 5️⃣ Verify password
        //        // 5️⃣ Verify password
        //        bool isPasswordValid = false;

        //        try
        //        {
        //            // inputPassword = "Manpal@123"
        //            // user.PasswordHash = "$2a$11$RUG2FV1nULupr4fp7x8WR.RiAJHCB/vP9FmtILiUlybarFe/7Dote"



        //            isPasswordValid = BCrypt.Net.BCrypt.Verify(inputPassword, user.PasswordHash);

        //            _logger.LogInformation("Password verification result for {Username}: {Result}", user.Username, isPasswordValid);
        //        }
        //        catch (Exception ex)
        //        {
        //            _logger.LogError(ex, "BCrypt library encountered an error during verification.");
        //            return new LoginResponse { Success = false, Message = "Internal verification error" };
        //        }

        //        // Check the boolean result
        //        if (!isPasswordValid)
        //        {
        //            _logger.LogWarning("Verification failed: Password does not match the stored hash.");
        //            return new LoginResponse
        //            {
        //                Success = false,
        //                Message = "Invalid username/email or password"
        //            };
        //        }
        //        // 6️⃣ Success Flow: Generate Tokens
        //        var refreshToken = _jwtService.GenerateRefreshToken();
        //        var refreshTokenExpiry = DateTime.UtcNow.AddDays(Convert.ToInt32(_configuration["Jwt:RefreshTokenExpiryInDays"]));

        //        user.RefreshToken = refreshToken;
        //        user.RefreshTokenExpiry = refreshTokenExpiry;
        //        await _userRepository.UpdateUserAsync(user);

        //        var token = _jwtService.GenerateToken(user);
        //        var tokenExpiry = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:TokenExpiryInMinutes"]));

        //        _logger.LogInformation("User logged in successfully: {Username}", user.Username);

        //        return new LoginResponse
        //        {
        //            Success = true,
        //            Message = "Login successful",
        //            Token = token,
        //            RefreshToken = refreshToken,
        //            TokenExpiry = tokenExpiry,
        //            RefreshTokenExpiry = refreshTokenExpiry,
        //            User = new UserProfile
        //            {
        //                Id = user.Id,
        //                Username = user.Username,
        //                Email = user.Email,
        //                FullName = user.FullName,
        //                IsActive = user.IsActive,
        //                CreatedAt = user.CreatedAt
        //            }
        //        };
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Unexpected login error for: {Identifier}", request.UsernameOrEmail);
        //        return new LoginResponse { Success = false, Message = "An internal error occurred." };
        //    }
        //}

        public async Task<LoginResponse> RefreshTokenAsync(RefreshTokenRequest request)
        {
            try
            {
                var principal = _jwtService.GetPrincipalFromExpiredToken(request.Token ?? "");
                var userIdClaim = principal?.FindFirst("userId")?.Value;
                if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid token"
                    };
                }

                var isValidRefreshToken = await _jwtService.ValidateRefreshTokenAsync(userId, request.RefreshToken ?? "");
                if (!isValidRefreshToken)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid refresh token"
                    };
                }

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "User not found"
                    };
                }
                var newToken = _jwtService.GenerateToken(user);
                var newRefreshToken = _jwtService.GenerateRefreshToken();
                var newRefreshTokenExpiry = DateTime.UtcNow.AddDays(
                    Convert.ToInt32(_configuration["Jwt:RefreshTokenExpiryInDays"])
                );

                user.RefreshToken = newRefreshToken;
                user.RefreshTokenExpiry = newRefreshTokenExpiry;
                await _userRepository.UpdateUserAsync(user);
                return new LoginResponse
                {
                    Success = true,
                    Message = "Token refreshed successfully",
                    Token = newToken,
                    RefreshToken = newRefreshToken,
                    TokenExpiry = DateTime.UtcNow.AddMinutes(
                        Convert.ToDouble(_configuration["Jwt:TokenExpiryInMinutes"])
                    ),
                    RefreshTokenExpiry = newRefreshTokenExpiry,
                    User = new UserProfile
                    {
                        Id = user.Id,
                        Username = user.Username,
                        Email = user.Email,
                        FullName = user.FullName,
                        PhoneNumber = user.PhoneNumber,
                        IsEmailVerified = user.IsEmailVerified,
                        IsActive = user.IsActive,
                        CreatedAt = user.CreatedAt
                    }
                };
            }
            catch (SecurityTokenException ex)
            {
                _logger.LogError(ex, "Token validation error during refresh");
                return new LoginResponse
                {
                    Success = false,
                    Message = "Invalid token"
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing token");
                return new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred while refreshing the token. Please try again later."
                };

            }
        }



        public async Task<bool> LogoutAsync(int userId)
        {
            try
            {

                return await _jwtService.RevokeRefreshTokenAsync(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error logging out user ID: {UserId}", userId);
                return false;
            }
        }


      

        public async Task<bool> CheckUsernameAvailabilityAsync(string username)
        {
            try
            {
                var user = await _userRepository.GetUserByUsernameAsync(username);
                return user == null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username availability: {Username}", username);
                throw;
            }
        }

        public async Task<bool> CheckEmailAvailabilityAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                return user == null;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email availability: {Email}", email);
                throw;
            }
        }
        public async Task<bool> VerifyEmailAsync(EmailVerificationRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token))
                return false;

            // Get user by token
            var user = await _userRepository.GetByEmailVerificationTokenAsync(request.Token);

            if (user == null)
                return false;

            // Check expiry
            if (!user.EmailVerificationTokenExpiry.HasValue ||
                user.EmailVerificationTokenExpiry < DateTime.UtcNow)
                return false;

            // ADO.NET style update (NO entity tracking)
            return await _userRepository.VerifyEmailAsync(user.Id);
        }

        // Additional methods for profile management
        public async Task<bool> UpdateProfileAsync(int userId, UpdateProfileRequest request)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                user.FullName = request.FullName?.Trim();
                user.PhoneNumber = request.PhoneNumber?.Trim();

                return await _userRepository.UpdateUserAsync(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // Verify current password
                if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword ?? "", user.PasswordHash ?? ""))
                {
                    return false;
                }

                // Validate new password strength
                if (!IsPasswordStrong(request.NewPassword ?? ""))
                {
                    return false;
                }

                // Hash new password
                var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword ?? "");

                // Update password
                return await _userRepository.UpdatePasswordAsync(userId, newPasswordHash);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user ID: {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Email))
                return false;

            // 1. Check user exists
            var user = await _userRepository.GetUserByEmailAsync(request.Email);

            if (user== null)
                return false;

            // 2. Generate secure token
            var resetToken = Guid.NewGuid().ToString("N");
            var expiry = DateTime.UtcNow.AddHours(1);

            // 3. Save token
            await _userRepository.SavePasswordResetTokenAsync(
                user.Id,
                resetToken,
                expiry
            );

            //// 4. Send email (SMTP / service)
            //await _emailService.SendPasswordResetEmailAsync(
            //    request.Email,
            //    resetToken
            //);

            return true;
        }
        public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.Token) ||
                string.IsNullOrWhiteSpace(request.NewPassword))
                return false;

            // 1. Get user by reset token
            var user = await _userRepository.GetUserByPasswordResetTokenAsync(request.Token);

            if (user == null)
                return false;

            // 2. Check token expiry
            if (!user.PasswordResetTokenExpiry.HasValue ||
                user.PasswordResetTokenExpiry < DateTime.UtcNow)
                return false;
            string passwordHash;
            using (var sha = SHA256.Create())
            {
                var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(request.NewPassword ?? ""));
                passwordHash = Convert.ToBase64String(bytes);
            }
            // 3. Hash password
            //var passwordHash = PasswordHasher.Hash(request.NewPassword);

            // 4. Update password (ADO.NET way)
            return await _userRepository.UpdatePasswordAsync(
                user.Id,
                passwordHash
            );
        }


        //public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordRequest request)
        //{
        //    try
        //    {
        //        var user = await _userRepository.GetUserByIdAsync(userId);
        //        if (user == null)
        //        {
        //            return false;
        //        }
        //        // Verify current password
        //        if (!BCrypt.Net.BCrypt.Verify(request.CurrentPassword, user.PasswordHash))
        //        {
        //            return false;
        //        }
        //        // Validate new password strength
        //        if (!IsPasswordStrong(request.NewPassword))
        //        {
        //            return false;
        //        }
        //        // Hash new password
        //        var newPasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        //        user.PasswordHash = newPasswordHash;
        //        user.UpdatedAt = DateTime.UtcNow;
        //        // Update password
        //        return await _userRepository.UpdateUserAsync(user);
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error changing password for user ID: {UserId}", userId);
        //        throw;
        //    }
        //}

        private bool IsPasswordStrong(string password)
        {
            // Minimum 8 characters
            if (password.Length < 8) return false;

            // At least one uppercase letter
            if (!password.Any(char.IsUpper)) return false;

            // At least one lowercase letter
            if (!password.Any(char.IsLower)) return false;

            // At least one digit
            if (!password.Any(char.IsDigit)) return false;

            // At least one special character
            if (!password.Any(ch => !char.IsLetterOrDigit(ch))) return false;

            return true;
        }




    }
}
