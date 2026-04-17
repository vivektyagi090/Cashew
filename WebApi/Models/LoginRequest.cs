using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class LoginRequest
    {
        [Required(ErrorMessage = "Username or email is required")]
        public string? UsernameOrEmail { get; set; }

        [Required(ErrorMessage = "Password is required")]
        public string? Password { get; set; }

        public bool RememberMe { get; set; }
    }
}
