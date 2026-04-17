using Microsoft.Data.SqlClient;
using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Data;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Repository.Impliment
{
    public class ReviewRepository : IReviewRepository
    {
        private readonly DbConnectionFactory _db;
        public ReviewRepository(DbConnectionFactory db) { _db = db; }

        public async Task<IEnumerable<ReviewModel>> GetProductReviewsAsync(int productId)
        {
            var list = new List<ReviewModel>();
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"SELECT r.ReviewId, r.ProductId, r.UserId, r.Rating, r.Comment, r.CreatedAt,
                                       u.FullName
                                       FROM Reviews r
                                       JOIN Users u ON r.UserId = u.Id
                                       WHERE r.ProductId = @ProductId
                                       ORDER BY r.CreatedAt DESC", conn);
            cmd.Parameters.AddWithValue("@ProductId", productId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(new ReviewModel
                {
                    ReviewId = reader.GetInt32(0),
                    ProductId = reader.GetInt32(1),
                    UserId = reader.GetInt32(2),
                    Rating = reader.GetDecimal(3),
                    Comment = reader.IsDBNull(4) ? null : reader.GetString(4),
                    CreatedAt = reader.GetDateTime(5),
                    UserName = reader.IsDBNull(6) ? null : reader.GetString(6)
                });
            }
            return list;
        }

        public async Task<bool> AddReviewAsync(ReviewModel review)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"INSERT INTO Reviews (ProductId, UserId, Rating, Comment)
                                       VALUES (@ProductId, @UserId, @Rating, @Comment);
                                       -- Update Product average rating
                                       UPDATE Products 
                                       SET Rating = (SELECT AVG(Rating) FROM Reviews WHERE ProductId = @ProductId),
                                           ReviewCount = (SELECT COUNT(*) FROM Reviews WHERE ProductId = @ProductId)
                                       WHERE ProductId = @ProductId;", conn);
            cmd.Parameters.AddWithValue("@ProductId", review.ProductId);
            cmd.Parameters.AddWithValue("@UserId", review.UserId);
            cmd.Parameters.AddWithValue("@Rating", review.Rating);
            cmd.Parameters.AddWithValue("@Comment", (object?)review.Comment ?? DBNull.Value);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteReviewAsync(int reviewId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand("DELETE FROM Reviews WHERE ReviewId = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", reviewId);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }
    }
}
