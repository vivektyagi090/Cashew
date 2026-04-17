using Microsoft.Data.SqlClient;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Repository.Impliment
{
    public class CouponRepository : ICouponRepository
    {
        private readonly DbConnectionFactory _db;
        public CouponRepository(DbConnectionFactory db) { _db = db; }

        public async Task<CouponModel?> GetCouponByCodeAsync(string code)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"SELECT CouponId, Code, DiscountPercentage, MaxDiscount, ExpiryDate, IsActive, CreatedAt
                                       FROM Coupons
                                       WHERE Code = @Code AND IsActive = 1 AND ExpiryDate > GETDATE()", conn);
            cmd.Parameters.AddWithValue("@Code", code);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync())
            {
                return new CouponModel
                {
                    CouponId = reader.GetInt32(0),
                    Code = reader.GetString(1),
                    DiscountPercentage = reader.GetDecimal(2),
                    MaxDiscount = reader.GetDecimal(3),
                    ExpiryDate = reader.GetDateTime(4),
                    IsActive = reader.GetBoolean(5),
                    CreatedAt = reader.GetDateTime(6)
                };
            }
            return null;
        }

        public async Task<bool> AddCouponAsync(CouponModel coupon)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"INSERT INTO Coupons (Code, DiscountPercentage, MaxDiscount, ExpiryDate, IsActive)
                                       VALUES (@Code, @Pct, @Max, @Expiry, @Active)", conn);
            cmd.Parameters.AddWithValue("@Code", coupon.Code);
            cmd.Parameters.AddWithValue("@Pct", coupon.DiscountPercentage);
            cmd.Parameters.AddWithValue("@Max", coupon.MaxDiscount);
            cmd.Parameters.AddWithValue("@Expiry", coupon.ExpiryDate);
            cmd.Parameters.AddWithValue("@Active", coupon.IsActive);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateCouponAsync(CouponModel coupon)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"UPDATE Coupons SET DiscountPercentage=@Pct, MaxDiscount=@Max, ExpiryDate=@Expiry, IsActive=@Active
                                       WHERE CouponId = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", coupon.CouponId);
            cmd.Parameters.AddWithValue("@Pct", coupon.DiscountPercentage);
            cmd.Parameters.AddWithValue("@Max", coupon.MaxDiscount);
            cmd.Parameters.AddWithValue("@Expiry", coupon.ExpiryDate);
            cmd.Parameters.AddWithValue("@Active", coupon.IsActive);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteCouponAsync(int couponId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand("UPDATE Coupons SET IsActive = 0 WHERE CouponId = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", couponId);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}
