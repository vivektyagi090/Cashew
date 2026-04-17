using System;

namespace WebApi.Models
{
    public class ReviewModel
    {
        public int ReviewId { get; set; }
        public int ProductId { get; set; }
        public int UserId { get; set; }
        public decimal Rating { get; set; }
        public string? Comment { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties
        public string? UserName { get; set; }
    }
}
