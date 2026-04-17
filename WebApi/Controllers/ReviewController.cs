using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReviewController : ControllerBase
    {
        private readonly IReviewRepository _reviewRepository;
        public ReviewController(IReviewRepository reviewRepository)
        {
            _reviewRepository = reviewRepository;
        }

        [HttpGet("product/{productId}")]
        public async Task<IActionResult> GetProductReviews(int productId)
        {
            var reviews = await _reviewRepository.GetProductReviewsAsync(productId);
            return Ok(reviews);
        }

        [Authorize]
        [HttpPost("add")]
        public async Task<IActionResult> AddReview([FromBody] ReviewModel review)
        {
            var userId = GetCurrentUserId();
            if (userId == 0) return Unauthorized();
            
            review.UserId = userId;
            var result = await _reviewRepository.AddReviewAsync(review);
            if (result) return Ok(new { Message = "Review added" });
            return BadRequest(new { Message = "Failed to add review" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{reviewId}")]
        public async Task<IActionResult> DeleteReview(int reviewId)
        {
            var result = await _reviewRepository.DeleteReviewAsync(reviewId);
            if (result) return Ok(new { Message = "Review deleted" });
            return BadRequest(new { Message = "Failed to delete review" });
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
