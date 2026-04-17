using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CouponController : ControllerBase
    {
        private readonly ICouponRepository _couponRepository;
        public CouponController(ICouponRepository couponRepository)
        {
            _couponRepository = couponRepository;
        }

        [HttpGet("validate/{code}")]
        public async Task<IActionResult> ValidateCoupon(string code)
        {
            var coupon = await _couponRepository.GetCouponByCodeAsync(code);
            if (coupon != null)
            {
                return Ok(new 
                { 
                    Valid = true, 
                    DiscountPercentage = coupon.DiscountPercentage,
                    MaxDiscount = coupon.MaxDiscount
                });
            }
            return NotFound(new { Valid = false, Message = "Invalid or expired coupon" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateCoupon([FromBody] CouponModel coupon)
        {
            var result = await _couponRepository.AddCouponAsync(coupon);
            if (result) return Ok(new { Message = "Coupon created" });
            return BadRequest(new { Message = "Failed to create coupon" });
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateCoupon([FromBody] CouponModel coupon)
        {
            var result = await _couponRepository.UpdateCouponAsync(coupon);
            if (result) return Ok(new { Message = "Coupon updated" });
            return BadRequest(new { Message = "Failed to update coupon" });
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("{couponId}")]
        public async Task<IActionResult> DeleteCoupon(int couponId)
        {
            var result = await _couponRepository.DeleteCouponAsync(couponId);
            if (result) return Ok(new { Message = "Coupon deleted" });
            return BadRequest(new { Message = "Failed to delete coupon" });
        }
    }
}
