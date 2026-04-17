using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Repository.Impliment
{
    public class WishlistRepository : IWishlistRepository
    {
        private readonly DbConnectionFactory _db;
        public WishlistRepository(DbConnectionFactory db) { _db = db; }

        public async Task<IEnumerable<WishlistModel>> GetUserWishlistAsync(int userId)
        {
            var list = new List<WishlistModel>();
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"SELECT w.WishlistId, w.UserId, w.ProductId, w.CreatedAt,
                                       p.Name, p.Price, p.ImageUrl
                                       FROM Wishlists w
                                       JOIN Products p ON w.ProductId = p.ProductId
                                       WHERE w.UserId = @UserId", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new WishlistModel
                {
                    WishlistId = reader.GetInt32(0),
                    UserId = reader.GetInt32(1),
                    ProductId = reader.GetInt32(2),
                    CreatedAt = reader.GetDateTime(3),
                    ProductName = reader.GetString(4),
                    ProductPrice = reader.GetDecimal(5),
                    ProductImageUrl = reader.IsDBNull(6) ? null : reader.GetString(6)
                });
            }
            return list;
        }

        public async Task<bool> AddToWishlistAsync(WishlistModel wishlist)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"IF NOT EXISTS (SELECT 1 FROM Wishlists WHERE UserId = @UserId AND ProductId = @ProductId)
                                       INSERT INTO Wishlists (UserId, ProductId) VALUES (@UserId, @ProductId)", conn);
            cmd.Parameters.AddWithValue("@UserId", wishlist.UserId);
            cmd.Parameters.AddWithValue("@ProductId", wishlist.ProductId);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> RemoveFromWishlistAsync(int wishlistId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand("DELETE FROM Wishlists WHERE WishlistId = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", wishlistId);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> IsInWishlistAsync(int userId, int productId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand("SELECT COUNT(1) FROM Wishlists WHERE UserId = @UserId AND ProductId = @ProductId", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            cmd.Parameters.AddWithValue("@ProductId", productId);
            return (int)(await cmd.ExecuteScalarAsync())! > 0;
        }
    }
}
