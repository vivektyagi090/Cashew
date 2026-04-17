using Microsoft.Data.SqlClient;
using WebApi.Data;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Repository.Impliment
{
    public class CategoryRepository : ICategoryRepository
    {
        private readonly DbConnectionFactory _db;

        public CategoryRepository(DbConnectionFactory db)
        {
            _db = db;
        }

        public async Task<List<CategoryModel>> GetAllCategoriesAsync()
        {
            var list = new List<CategoryModel>();
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand("SELECT CategoryId, Name, Description, ImageUrl, IsActive, CreatedAt FROM Categories WHERE IsActive = 1", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
            {
                list.Add(MapCategory(reader));
            }
            return list;
        }

        public async Task<CategoryModel?> GetCategoryByIdAsync(int categoryId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand("SELECT CategoryId, Name, Description, ImageUrl, IsActive, CreatedAt FROM Categories WHERE CategoryId = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", categoryId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) return MapCategory(reader);
            return null;
        }

        public async Task<int> CreateCategoryAsync(CategoryModel category)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"INSERT INTO Categories (Name, Description, ImageUrl) 
                                       OUTPUT INSERTED.CategoryId 
                                       VALUES (@Name, @Desc, @Img)", conn);
            cmd.Parameters.AddWithValue("@Name", category.Name);
            cmd.Parameters.AddWithValue("@Desc", (object?)category.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Img",  (object?)category.ImageUrl   ?? DBNull.Value);
            return (int)(await cmd.ExecuteScalarAsync())!;
        }

        public async Task<bool> UpdateCategoryAsync(CategoryModel category)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand("UPDATE Categories SET Name=@Name, Description=@Desc, ImageUrl=@Img WHERE CategoryId=@Id", conn);
            cmd.Parameters.AddWithValue("@Name", category.Name);
            cmd.Parameters.AddWithValue("@Desc", (object?)category.Description ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Img",  (object?)category.ImageUrl   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Id",   category.CategoryId);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteCategoryAsync(int categoryId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand("UPDATE Categories SET IsActive = 0 WHERE CategoryId = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", categoryId);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private static CategoryModel MapCategory(SqlDataReader r) => new()
        {
            CategoryId  = r.GetInt32(0),
            Name        = r.GetString(1),
            Description = r.IsDBNull(2) ? null : r.GetString(2),
            ImageUrl    = r.IsDBNull(3) ? null : r.GetString(3),
            IsActive    = r.GetBoolean(4),
            CreatedAt   = r.GetDateTime(5)
        };
    }
}
