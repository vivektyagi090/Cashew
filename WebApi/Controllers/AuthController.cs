using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class AuthController : ControllerBase
    {
        private readonly IUserRepository _userRepository;
        private readonly IAuthService _authService;
        private readonly ILogger<AuthController> _logger;
        public AuthController(IAuthService authService, ILogger<AuthController> logger, IUserRepository userRepository)
        {
            _userRepository = userRepository;
            _authService = authService;
            _logger = logger;
        }
        [HttpPost("register")]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<IActionResult> Register([FromBody] Models.RegisterRequest request)
        {
            try
            {
                _logger.LogInformation("Registering user: {Username}", request.Username);

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage).ToList();
                    return BadRequest(new RegisterResponse { Success = false, Message = "Invalid input", UserId = 0 });
                }

                //if (request.Password != request.ConfirmPassword)
                //{
                //    return BadRequest(new RegisterResponse
                //    {
                //        Success = false,
                //        Message = "Passwords do not match",
                //        UserId = 0
                //    });
                //}


                //var userExists = await _userRepository.UserExistsAsync(request.Username, request.Email);
                //if (userExists)
                //{
                //    return BadRequest(new RegisterResponse
                //    {
                //        Success = false,
                //        Message = "Username or email already exists",
                //        UserId = 0
                //    });
                //}


                //if (!IsPasswordStrong(request.Password))
                //{
                //    return BadRequest(new RegisterResponse
                //    {
                //        Success = false,
                //        Message = "Password must contain at least 8 characters, including uppercase, lowercase, numbers, and special characters",
                //        UserId = 0
                //    });
                //}

                var result = await _authService.RegisterAsync(request);
                if (result.Success)
                {
                    _logger.LogInformation("User registered successfully: {Username} (ID: {UserId})", result.Username, result.UserId);
                    return Ok(result);

                }
                else
                {
                    _logger.LogWarning("User registration failed: {Message}", result.Message);
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during user registration: {Username}", request.Username);
                return StatusCode(StatusCodes.Status500InternalServerError, new RegisterResponse
                {
                    Success = false,
                    Message = "An error occurred during registration. Please try again later.",
                    UserId = 0
                });

            }
        }


        [HttpGet("check-username/{username}")]
        [ProducesResponseType(typeof(AvailabilityResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckUsernameAvailability(string username)
        {
            try
            {
                var isAvailable = await _authService.CheckUsernameAvailabilityAsync(username);

                return Ok(new AvailabilityResponse
                {
                    Available = isAvailable,
                    Message = isAvailable ? "Username is available" : "Username is already taken"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking username availability: {Username}", username);

                return StatusCode(StatusCodes.Status500InternalServerError, new AvailabilityResponse
                {
                    Available = false,
                    Message = "Error checking username availability"
                });
            }
        }


        [HttpGet("check-email/{email}")]
        [ProducesResponseType(typeof(AvailabilityResponse), StatusCodes.Status200OK)]
        public async Task<IActionResult> CheckEmailAvailability(string email)
        {
            try
            {
                var isAvailable = await _authService.CheckEmailAvailabilityAsync(email);

                return Ok(new AvailabilityResponse
                {
                    Available = isAvailable,
                    Message = isAvailable ? "Email is available" : "Email is already registered"
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error checking email availability: {Email}", email);

                return StatusCode(StatusCodes.Status500InternalServerError, new AvailabilityResponse
                {
                    Available = false,
                    Message = "Error checking email availability"
                });
            }
        }




        [Authorize]
        [HttpPut("change-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ChangesPassword([FromBody] ChangePasswordRequest request)
        {
            int userId = 0;
            try
            {
                userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { Message = "User not authenticated" });
                }
                // Validate confirm password
                if (request.NewPassword != request.ConfirmNewPassword)
                {
                    return BadRequest(new { Message = "New passwords do not match" });
                }
                var result = await _authService.ChangePasswordAsync(userId, request);
                if (result)
                {
                    return Ok(new { Message = "Password changed successfully" });
                }
                return BadRequest(new { Message = "Failed to change password. Please check your current password." });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error changing password for user ID: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error changing password" });
            }
        }





        //[HttpPut("change-password/{userId}")]
        //[ProducesResponseType(StatusCodes.Status200OK)]
        //[ProducesResponseType(StatusCodes.Status400BadRequest)]
        //public async Task<IActionResult> ChangePassword(int userId, [FromBody] ChangePasswordRequest request)
        //{
        //    try
        //    {
        //        // Validate confirm password
        //        if (request.NewPassword != request.ConfirmNewPassword)
        //        {
        //            return BadRequest(new { Message = "New passwords do not match" });
        //        }

        //        var result = await _authService.ChangePasswordAsync(userId, request);

        //        if (result)
        //        {
        //            return Ok(new { Message = "Password changed successfully" });
        //        }

        //        return BadRequest(new { Message = "Failed to change password. Please check your current password." });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error changing password for user ID: {UserId}", userId);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error changing password" });
        //    }
        //}

        [HttpGet("profile/{userId}")]
        [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetProfile(int userId)
        {
            try
            {
                // This would typically use a UserService, but for simplicity:
                // var user = await _userRepository.GetUserByIdAsync(userId);
                // if (user == null) return NotFound();
                // return Ok(new UserProfileResponse { ... });

                return Ok(new { Message = "Get profile endpoint - implement as needed" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile for user ID: {UserId}", userId);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPost("login")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> Login([FromBody] LoginRequest request)
        {
            try
            {
                _logger.LogInformation("Login request received for identifier: {Identifier}", request.UsernameOrEmail);
                if (!ModelState.IsValid)
                {
                    return BadRequest(new LoginResponse
                    {
                        Success = false,
                        Message = "Invalid input",
                        Token = null,
                        RefreshToken = null  
                    });
                }
                var result = await _authService.LoginAsync(request);
                if (result.Success)
                {

                    _logger.LogInformation("User logged in successfully: {Identifier}", result.User?.Username);
                    Response.Cookies.Append("refreshToken", result.RefreshToken ?? "", new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = result.RefreshTokenExpiry
                    });

                    return Ok(new
                    {

                        result.Success,
                        result.Message,
                        result.Token,
                        result.TokenExpiry,
                        result.User


                    });
                }
                else
                {
                    _logger.LogWarning("Login failed for identifier: {Identifier}", request.UsernameOrEmail);
                    return Unauthorized(result);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during login for identifier: {Identifier}", request.UsernameOrEmail);
                return StatusCode(StatusCodes.Status500InternalServerError, new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during login. Please try again later.",
                    Token = null,
                    RefreshToken = null
                });
            }
        }

        [HttpPost("refresh-token")]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status401Unauthorized)]

        public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest request)
        {
            try
            {
                if (request == null)
                {
                    return BadRequest(new LoginResponse { Success = false, Message = "Request is required" });
                }

                if (string.IsNullOrEmpty(request.RefreshToken))
                {
                    request.RefreshToken = Request.Cookies["refreshToken"] ?? "";
                }

                if (string.IsNullOrEmpty(request.Token) || string.IsNullOrEmpty(request.RefreshToken))
                {
                    return Unauthorized(new LoginResponse
                    {
                        Success = false,
                        Message = "Token is required"
                    });
                }
                var result = await _authService.RefreshTokenAsync(request);
                if (result.Success)
                {
                    Response.Cookies.Append("refreshToken", result.RefreshToken ?? "", new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                        Expires = result.RefreshTokenExpiry
                    });

                    return Ok(new
                    {
                        result.Success,
                        result.Message,
                        result.Token,
                        result.TokenExpiry,
                        result.User
                    });
                }
                else
                {
                    return Unauthorized(result);
                }

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during token refresh");
                return StatusCode(StatusCodes.Status500InternalServerError, new LoginResponse
                {
                    Success = false,
                    Message = "An error occurred during token refresh. Please try again later.",
                    Token = null,
                    RefreshToken = null
                });
            }
        }

        [Authorize]
        [HttpPost("logout")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }
                var result = await _authService.LogoutAsync(userId);
                Response.Cookies.Delete("refreshToken");
                if (result)
                {
                    _logger.LogInformation("User logged out successfully: User ID {UserId}", userId);
                    return Ok(new { message = "Logged out successfully" });
                }
                else
                {
                    return BadRequest(new { message = "Logout failed" });
                }
            }
            catch (Exception ex)
            {

                _logger.LogError(ex, "Error during logout");
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Logout failed" });
            }
        }

        [Authorize]
        [HttpGet("profile")]
        [ProducesResponseType(typeof(UserProfileResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> Getprofile()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized(new { message = "User not authenticated" });
                }
                var username = User.FindFirst("username")?.Value;
                var email = User.FindFirst("email")?.Value;
                // Implement profile retrieval logic here
                return Ok(new { UserId = userId, Username = username ?? "", Email = email ?? "" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting profile ");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        //[HttpGet("check-username/{username}")]
        //[ProducesResponseType(typeof(AvailabilityResponse), StatusCodes.Status200OK)]
        //public async Task<IActionResult> CheckUsernameAvailability(string username)
        //{
        //    try
        //    {
        //        var isAvailable = await _authService.CheckUsernameAvailabilityAsync(username);
        //        return Ok(new AvailabilityResponse
        //        {
        //            Available = isAvailable,
        //            Message = isAvailable ? "Username is available" : "Username is already taken"
        //        });
        //    }
        //    catch (Exception ex)
        //    {
        //        _logger.LogError(ex, "Error checking username availability: {Username}", username);
        //        return StatusCode(StatusCodes.Status500InternalServerError, new AvailabilityResponse
        //        {
        //            Available = false,
        //            Message = "Error checking username availability"
        //        });
        //    }
      //  }

        [HttpPost("verify-email")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> VerifyEmail([FromBody] EmailVerificationRequest request)
        {
            try
            {
                var result = await _authService.VerifyEmailAsync(request);
                if (result)
                {
                    return Ok(new { Message = "Email verified successfully" });
                }
                else
                {
                    return BadRequest(new { Message = "Invalid or expired verification token" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during email verification for token: {Token}", request.Token);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error verifying email" });
            }
        }

        [HttpPost("forgot-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
        {
            try
            {
                var result = await _authService.ForgotPasswordAsync(request);
                if (result)
                {
                    return Ok(new { Message = "Password reset link sent to your email" });
                }
                else
                {
                    return BadRequest(new { Message = "Email not found" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during forgot password for email: {Email}", request.Email);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error processing forgot password request" });
            }
        }
        [HttpPost("reset-password")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
        {
            try
            {
                if (request.NewPassword != request.ConfirmNewPassword)
                {
                    return BadRequest(new { Message = "Passwords do not match" });
                }
                var result = await _authService.ResetPasswordAsync(request);
                if (result)
                {
                    return Ok(new { Message = "Password reset successfully" });
                }
                else
                {
                    return BadRequest(new { Message = "Invalid or expired reset token" });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error during password reset for token: {Token}", request.Token);
                return StatusCode(StatusCodes.Status500InternalServerError, new { Message = "Error resetting password" });
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId");
            if (userIdClaim != null && int.TryParse(userIdClaim.Value, out int userId))
            {
                return userId;
            }
            return 0;
        }

        // Supporting request classes
        //public class ForgotPasswordRequest
        //{
        //    public string Email { get; set; }
        //}

        //public class ResetPasswordRequest
        //{
        //    public string Token { get; set; }
        //    public string NewPassword { get; set; }
        //    public string ConfirmNewPassword { get; set; }
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
