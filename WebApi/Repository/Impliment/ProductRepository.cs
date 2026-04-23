using Microsoft.Data.SqlClient;
using WebApi.Data;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Repository.Impliment
{
    public class ProductRepository : IProductRepository
    {
        private readonly DbConnectionFactory _db;
        public ProductRepository(DbConnectionFactory db) { _db = db; }

        public async Task<ProductListResponse> GetProductsAsync(ProductListRequest req)
        {
            var products = new List<ProductModel>();
            int totalCount = 0;

            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            var where = new List<string> { "p.IsActive = 1" };
            var cmdParams = new List<SqlParameter>();

            if (req.CategoryId.HasValue)
            {
                where.Add("p.CategoryId = @CatId");
                cmdParams.Add(new SqlParameter("@CatId", req.CategoryId.Value));
            }
            if (!string.IsNullOrWhiteSpace(req.Search))
            {
                where.Add("(p.Name LIKE @Search OR p.Description LIKE @Search OR p.Brand LIKE @Search)");
                cmdParams.Add(new SqlParameter("@Search", $"%{req.Search}%"));
            }
            if (req.MinPrice.HasValue)
            {
                where.Add("p.Price >= @MinPrice");
                cmdParams.Add(new SqlParameter("@MinPrice", req.MinPrice.Value));
            }
            if (req.MaxPrice.HasValue)
            {
                where.Add("p.Price <= @MaxPrice");
                cmdParams.Add(new SqlParameter("@MaxPrice", req.MaxPrice.Value));
            }

            string whereClause = string.Join(" AND ", where);
            string orderBy = req.SortBy switch
            {
                "price_asc"  => "p.Price ASC",
                "price_desc" => "p.Price DESC",
                "rating"     => "p.Rating DESC",
                _            => "p.CreatedAt DESC"
            };

            int offset = (req.Page - 1) * req.PageSize;

            // Count
            var countCmd = new SqlCommand($"SELECT COUNT(*) FROM Products p WHERE {whereClause}", conn);
            countCmd.Parameters.AddRange(cmdParams.Select(p => new SqlParameter(p.ParameterName, p.Value)).ToArray());
            totalCount = (int)(await countCmd.ExecuteScalarAsync())!;

            // Data
            string sql = $@"SELECT p.ProductId, p.CategoryId, c.Name AS CategoryName, p.Name, p.Description,
                            p.Price, p.OriginalPrice, p.StockQty, p.ImageUrl, p.Rating, p.ReviewCount,
                            p.Brand, p.IsActive, p.IsFeatured, p.CreatedAt
                            FROM Products p
                            LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                            WHERE {whereClause}
                            ORDER BY {orderBy}
                            OFFSET {offset} ROWS FETCH NEXT {req.PageSize} ROWS ONLY";

            var dataCmd = new SqlCommand(sql, conn);
            dataCmd.Parameters.AddRange(cmdParams.Select(p => new SqlParameter(p.ParameterName, p.Value)).ToArray());
            using var reader = await dataCmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                products.Add(MapProduct(reader));

            return new ProductListResponse { Products = products, TotalCount = totalCount, Page = req.Page, PageSize = req.PageSize };
        }

        public async Task<ProductModel?> GetProductByIdAsync(int productId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"SELECT p.ProductId, p.CategoryId, c.Name AS CategoryName, p.Name, p.Description,
                                       p.Price, p.OriginalPrice, p.StockQty, p.ImageUrl, p.Rating, p.ReviewCount,
                                       p.Brand, p.IsActive, p.IsFeatured, p.CreatedAt
                                       FROM Products p
                                       LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                                       WHERE p.ProductId = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", productId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (await reader.ReadAsync()) return MapProduct(reader);
            return null;
        }

        public async Task<List<ProductModel>> GetFeaturedProductsAsync()
        {
            var list = new List<ProductModel>();
            try {
                using var conn = _db.CreateConnection();
                await conn.OpenAsync();
                var cmd = new SqlCommand(@"SELECT TOP 8 p.ProductId, p.CategoryId, c.Name AS CategoryName, p.Name, p.Description,
                                           p.Price, p.OriginalPrice, p.StockQty, p.ImageUrl, p.Rating, p.ReviewCount,
                                           p.Brand, p.IsActive, p.IsFeatured, p.CreatedAt
                                           FROM Products p
                                           LEFT JOIN Categories c ON p.CategoryId = c.CategoryId
                                           WHERE p.IsFeatured = 1 AND p.IsActive = 1
                                           ORDER BY p.Rating DESC", conn);
                using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync()) list.Add(MapProduct(reader));
            } catch (Exception ex) {
                // Log error or handle gracefully
                Console.WriteLine($"Error in GetFeaturedProductsAsync: {ex.Message}");
            }
            return list;
        }

        public async Task<int> CreateProductAsync(ProductModel p)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"INSERT INTO Products (CategoryId, Name, Description, Price, OriginalPrice, StockQty, ImageUrl, Brand, IsFeatured)
                                       OUTPUT INSERTED.ProductId
                                       VALUES (@CatId, @Name, @Desc, @Price, @OPrice, @Stock, @Img, @Brand, @Featured)", conn);
            cmd.Parameters.AddWithValue("@CatId",    p.CategoryId);
            cmd.Parameters.AddWithValue("@Name",     p.Name);
            cmd.Parameters.AddWithValue("@Desc",     (object?)p.Description   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Price",    p.Price);
            cmd.Parameters.AddWithValue("@OPrice",   (object?)p.OriginalPrice ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Stock",    p.StockQty);
            cmd.Parameters.AddWithValue("@Img",      (object?)p.ImageUrl      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Brand",    (object?)p.Brand         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Featured", p.IsFeatured);
            return (int)(await cmd.ExecuteScalarAsync())!;
        }

        public async Task<bool> UpdateProductAsync(ProductModel p)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"UPDATE Products SET CategoryId=@CatId, Name=@Name, Description=@Desc,
                                       Price=@Price, OriginalPrice=@OPrice, StockQty=@Stock,
                                       ImageUrl=@Img, Brand=@Brand, IsFeatured=@Featured
                                       WHERE ProductId=@Id", conn);
            cmd.Parameters.AddWithValue("@Id",       p.ProductId);
            cmd.Parameters.AddWithValue("@CatId",    p.CategoryId);
            cmd.Parameters.AddWithValue("@Name",     p.Name);
            cmd.Parameters.AddWithValue("@Desc",     (object?)p.Description   ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Price",    p.Price);
            cmd.Parameters.AddWithValue("@OPrice",   (object?)p.OriginalPrice ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Stock",    p.StockQty);
            cmd.Parameters.AddWithValue("@Img",      (object?)p.ImageUrl      ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Brand",    (object?)p.Brand         ?? DBNull.Value);
            cmd.Parameters.AddWithValue("@Featured", p.IsFeatured);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> DeleteProductAsync(int productId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand("UPDATE Products SET IsActive = 0 WHERE ProductId = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", productId);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        public async Task<bool> UpdateStockAsync(int productId, int quantity)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand("UPDATE Products SET StockQty = StockQty - @Qty WHERE ProductId = @Id AND StockQty >= @Qty", conn);
            cmd.Parameters.AddWithValue("@Qty", quantity);
            cmd.Parameters.AddWithValue("@Id",  productId);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private static ProductModel MapProduct(SqlDataReader r) => new()
        {
        ProductId     = r.GetInt32(0),
        CategoryId    = r.GetInt32(1),
        CategoryName  = r.IsDBNull(2) ? "" : r.GetString(2),
        Name          = r.IsDBNull(3) ? "" : r.GetString(3),
        Description   = r.IsDBNull(4) ? null : r.GetString(4),
        Price         = Convert.ToDecimal(r.GetValue(5)),
        OriginalPrice = r.IsDBNull(6) ? null : (decimal?)Convert.ToDecimal(r.GetValue(6)),
        StockQty      = r.GetInt32(7),
        ImageUrl      = r.IsDBNull(8) ? null : r.GetString(8),
        Rating        = Convert.ToDecimal(r.GetValue(9)),
        ReviewCount   = r.GetInt32(10),
        Brand         = r.IsDBNull(11) ? null : r.GetString(11),
        IsActive      = r.GetBoolean(12),
        IsFeatured    = r.GetBoolean(13),
        CreatedAt     = r.GetDateTime(14)
        };
    }
}
