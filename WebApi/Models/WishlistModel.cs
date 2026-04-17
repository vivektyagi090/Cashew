using System;

namespace WebApi.Models
{
    public class WishlistModel
    {
        public int WishlistId { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; }
        
        // Navigation properties (optional for DTOs but good for reference)
        public string? ProductName { get; set; }
        public decimal? ProductPrice { get; set; }
        public string? ProductImageUrl { get; set; }
    }
}
