using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class WishlistController : ControllerBase
    {
        private readonly IWishlistRepository _wishlistRepository;
        public WishlistController(IWishlistRepository wishlistRepository)
        {
            _wishlistRepository = wishlistRepository;
        }

        [HttpGet]
        public async Task<IActionResult> GetWishlist()
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();
            var wishlist = await _wishlistRepository.GetUserWishlistAsync(userId);
            return Ok(wishlist);
        }

        [HttpPost("add/{productId}")]
        public async Task<IActionResult> AddToWishlist(int productId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();
            
            var result = await _wishlistRepository.AddToWishlistAsync(new WishlistModel { UserId = userId, ProductId = productId });
            if (result) return Ok(new { Message = "Added to wishlist" });
            return BadRequest(new { Message = "Failed to add to wishlist or already exists" });
        }

        [HttpDelete("remove/{wishlistId}")]
        public async Task<IActionResult> RemoveFromWishlist(int wishlistId)
        {
            var result = await _wishlistRepository.RemoveFromWishlistAsync(wishlistId);
            if (result) return Ok(new { Message = "Removed from wishlist" });
            return BadRequest(new { Message = "Failed to remove from wishlist" });
        }

        [HttpGet("check/{productId}")]
        public async Task<IActionResult> IsInWishlist(int productId)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();
            var exists = await _wishlistRepository.IsInWishlistAsync(userId, productId);
            return Ok(new { IsInWishlist = exists });
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
    }
}
