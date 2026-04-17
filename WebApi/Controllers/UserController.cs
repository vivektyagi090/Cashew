using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class UserController : ControllerBase
    {
        private readonly ILogger<UserController> _logger;
        private readonly IUserRepository _userRepository;
        public UserController(ILogger<UserController> logger, IUserRepository userRepository)
        {
            _logger = logger;
            _userRepository = userRepository;
        }
        [Authorize(Roles = "Admin")]
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<UserProfile>), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        public async Task<IActionResult> GetAllUsers()
        {
            try
            {
                var (users, totalCount) = await _userRepository.GetAllUsersAsync();
                var userProfiles = users.Select(user => new UserProfile
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    IsEmailVerified = user.IsEmailVerified,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                }).ToList();
                return Ok(new
                {
                    Data = userProfiles,
                    TotalCount = totalCount
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting all users");
                return StatusCode(StatusCodes.Status500InternalServerError);
                throw;
            }


        }


        [HttpGet("{id}")]
        [ProducesResponseType(typeof(UserProfile), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetUserById(int id)
        {
            try
            {
                var user = await _userRepository.GetUserByIdAsync(id);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                // Check if current user is authorized to view this profile
                var currentUserId = GetCurrentUserId();
                if (currentUserId != id && !User.IsInRole("Admin"))
                {
                    return Forbid();
                }

                var userProfile = new UserProfile
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FullName = user.FullName,
                    PhoneNumber = user.PhoneNumber,
                    IsEmailVerified = user.IsEmailVerified,
                    IsActive = user.IsActive,
                    CreatedAt = user.CreatedAt
                };

                return Ok(userProfile);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error getting user by ID: {Id}", id);
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }


        [HttpPut("profile")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized();
                }

                var user = await _userRepository.GetUserByIdAsync(userId);
                if (user == null)
                {
                    return NotFound(new { Message = "User not found" });
                }

                user.FullName = request.FullName;
                user.PhoneNumber = request.PhoneNumber;
                user.UpdatedAt = DateTime.UtcNow;

                var updated = await _userRepository.UpdateUserAsync(user);
                if (updated)
                {
                    return Ok(new { Message = "Profile updated successfully" });
                }

                return BadRequest(new { Message = "Failed to update profile" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        [HttpPut("deactivate")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> DeactivateAccount()
        {
            try
            {
                var userId = GetCurrentUserId();
                if (userId == 0)
                {
                    return Unauthorized();
                }

                var deactivated = await _userRepository.DeactivateUserAsync(userId);
                if (deactivated)
                {
                    return Ok(new { Message = "Account deactivated successfully" });
                }

                return BadRequest(new { Message = "Failed to deactivate account" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating account");
                return StatusCode(StatusCodes.Status500InternalServerError);
            }
        }

        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirst("userId")?.Value;
            if (int.TryParse(userIdClaim, out int userId))
            {
                return userId;
            }
            return 0;
        }





        //public IActionResult Index()
        //{
        //    return View();
        //}

        //public async Task<IActionResult> CreateUser()
        //{
        //    // Logic to create a user
        //    return Ok();
        //}

        //public async Task<IActionResult> GetUserByEmail(string email)
        //{
        //    // Logic to get a user by email
        //    return Ok();
        //}


    }
}
