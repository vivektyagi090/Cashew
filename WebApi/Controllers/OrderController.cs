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
    public class OrderController : ControllerBase
    {
        private readonly IOrderRepository _repo;
        public OrderController(IOrderRepository repo) { _repo = repo; }

        private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "0");

        [HttpGet("my")]
        public async Task<IActionResult> GetMyOrders()
            => Ok(await _repo.GetOrdersByUserIdAsync(GetUserId()));

        [HttpGet("{id}")]
        public async Task<IActionResult> GetOrder(int id)
        {
            var order = await _repo.GetOrderByIdAsync(id);
            return order == null ? NotFound() : Ok(order);
        }

        [HttpGet("all")]
        public async Task<IActionResult> GetAllOrders()
            => Ok(await _repo.GetAllOrdersAsync());

        [HttpPost("place")]
        public async Task<IActionResult> PlaceOrder([FromBody] PlaceOrderRequest request)
        {
            try
            {
                var orderId = await _repo.PlaceOrderAsync(GetUserId(), request);
                return Ok(new { message = "Order placed successfully!", orderId });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        [HttpPut("status")]
        public async Task<IActionResult> UpdateStatus([FromBody] UpdateOrderStatusRequest request)
            => await _repo.UpdateOrderStatusAsync(request) ? Ok(new { message = "Status updated" }) : NotFound();
    }
}
