namespace WebApi.Models
{
    public class RegisterResponse
    {
        public bool Success { get; set; }
        public string? Message { get; set; }
        public int UserId { get; set; }
        public string? Username { get; set; }
        public string? Email { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? Token { get; set; }
        public string? RefreshToken { get; set; }
        public DateTime TokenExpiry { get; set; }
        public DateTime RefreshTokenExpiry { get; set; }
    }
}
