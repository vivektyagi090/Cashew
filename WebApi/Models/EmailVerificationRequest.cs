using System.ComponentModel.DataAnnotations;

namespace WebApi.Models
{
    public class EmailVerificationRequest
    {
        [Required]
        public string? Token { get; set; }
    }
}
