using System.Threading.Tasks;
using WebApi.Models;

namespace WebApi.Repository.Interface
{
    public interface ICouponRepository
    {
        Task<CouponModel?> GetCouponByCodeAsync(string code);
        Task<bool> AddCouponAsync(CouponModel coupon);
        Task<bool> UpdateCouponAsync(CouponModel coupon);
        Task<bool> DeleteCouponAsync(int couponId);
    }
}
