using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CartController : ControllerBase
    {
        private readonly ICartRepository _repo;
        public CartController(ICartRepository repo) { _repo = repo; }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        [HttpGet]
        public async Task<IActionResult> GetCart()
            => Ok(await _repo.GetCartByUserIdAsync(GetUserId()));

        [HttpPost("add")]
        public async Task<IActionResult> AddToCart([FromBody] AddToCartRequest request)
        {
            await _repo.AddToCartAsync(GetUserId(), request);
            return Ok(new { message = "Item added to cart" });
        }

        [HttpPut("update")]
        public async Task<IActionResult> UpdateItem([FromBody] UpdateCartItemRequest request)
        {
            await _repo.UpdateCartItemAsync(request);
            return Ok(new { message = "Cart updated" });
        }

        [HttpDelete("item/{cartItemId}")]
        public async Task<IActionResult> RemoveItem(int cartItemId)
        {
            await _repo.RemoveCartItemAsync(cartItemId);
            return Ok(new { message = "Item removed" });
        }

        [HttpDelete("clear")]
        public async Task<IActionResult> ClearCart()
        {
            await _repo.ClearCartAsync(GetUserId());
            return Ok(new { message = "Cart cleared" });
        }
    }
}
