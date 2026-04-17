using Microsoft.Data.SqlClient;
using WebApi.Data;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Repository.Impliment
{
    public class CartRepository : ICartRepository
    {
        private readonly DbConnectionFactory _db;
        public CartRepository(DbConnectionFactory db) { _db = db; }

        public async Task<CartModel?> GetCartByUserIdAsync(int userId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            // Get or create cart
            var cartCmd = new SqlCommand("SELECT CartId, UserId, CreatedAt, UpdatedAt FROM Cart WHERE UserId = @UserId", conn);
            cartCmd.Parameters.AddWithValue("@UserId", userId);
            using var cartReader = await cartCmd.ExecuteReaderAsync();

            CartModel? cart = null;
            if (await cartReader.ReadAsync())
            {
                cart = new CartModel
                {
                    CartId    = cartReader.GetInt32(0),
                    UserId    = cartReader.GetInt32(1),
                    CreatedAt = cartReader.GetDateTime(2),
                    UpdatedAt = cartReader.GetDateTime(3)
                };
            }
            cartReader.Close();

            if (cart == null) return null;

            // Get cart items
            var itemCmd = new SqlCommand(@"SELECT ci.CartItemId, ci.CartId, ci.ProductId, p.Name,
                                           p.ImageUrl, ci.Quantity, ci.Price
                                           FROM CartItems ci
                                           INNER JOIN Products p ON ci.ProductId = p.ProductId
                                           WHERE ci.CartId = @CartId", conn);
            itemCmd.Parameters.AddWithValue("@CartId", cart.CartId);
            using var itemReader = await itemCmd.ExecuteReaderAsync();
            while (await itemReader.ReadAsync())
            {
                cart.Items.Add(new CartItemModel
                {
                    CartItemId     = itemReader.GetInt32(0),
                    CartId         = itemReader.GetInt32(1),
                    ProductId      = itemReader.GetInt32(2),
                    ProductName    = itemReader.GetString(3),
                    ProductImageUrl= itemReader.IsDBNull(4) ? null : itemReader.GetString(4),
                    Quantity       = itemReader.GetInt32(5),
                    Price          = itemReader.GetDecimal(6)
                });
            }

            return cart;
        }

        public async Task AddToCartAsync(int userId, AddToCartRequest request)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            // Ensure cart exists
            var cartId = await EnsureCartAsync(conn, userId);

            // Check if item already in cart
            var checkCmd = new SqlCommand("SELECT CartItemId, Quantity FROM CartItems WHERE CartId = @CartId AND ProductId = @ProdId", conn);
            checkCmd.Parameters.AddWithValue("@CartId", cartId);
            checkCmd.Parameters.AddWithValue("@ProdId", request.ProductId);
            using var checkReader = await checkCmd.ExecuteReaderAsync();

            if (await checkReader.ReadAsync())
            {
                int existingId = checkReader.GetInt32(0);
                int existingQty = checkReader.GetInt32(1);
                checkReader.Close();
                var updateCmd = new SqlCommand("UPDATE CartItems SET Quantity = @Qty WHERE CartItemId = @Id", conn);
                updateCmd.Parameters.AddWithValue("@Qty", existingQty + request.Quantity);
                updateCmd.Parameters.AddWithValue("@Id", existingId);
                await updateCmd.ExecuteNonQueryAsync();
            }
            else
            {
                checkReader.Close();
                // Get price
                var priceCmd = new SqlCommand("SELECT Price FROM Products WHERE ProductId = @Id", conn);
                priceCmd.Parameters.AddWithValue("@Id", request.ProductId);
                var price = (decimal)(await priceCmd.ExecuteScalarAsync())!;

                var insertCmd = new SqlCommand("INSERT INTO CartItems (CartId, ProductId, Quantity, Price) VALUES (@CartId, @ProdId, @Qty, @Price)", conn);
                insertCmd.Parameters.AddWithValue("@CartId", cartId);
                insertCmd.Parameters.AddWithValue("@ProdId", request.ProductId);
                insertCmd.Parameters.AddWithValue("@Qty",    request.Quantity);
                insertCmd.Parameters.AddWithValue("@Price",  price);
                await insertCmd.ExecuteNonQueryAsync();
            }

            // Update cart timestamp
            var tsCmd = new SqlCommand("UPDATE Cart SET UpdatedAt = GETDATE() WHERE CartId = @Id", conn);
            tsCmd.Parameters.AddWithValue("@Id", cartId);
            await tsCmd.ExecuteNonQueryAsync();
        }

        public async Task UpdateCartItemAsync(UpdateCartItemRequest request)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand("UPDATE CartItems SET Quantity = @Qty WHERE CartItemId = @Id", conn);
            cmd.Parameters.AddWithValue("@Qty", request.Quantity);
            cmd.Parameters.AddWithValue("@Id",  request.CartItemId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task RemoveCartItemAsync(int cartItemId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand("DELETE FROM CartItems WHERE CartItemId = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", cartItemId);
            await cmd.ExecuteNonQueryAsync();
        }

        public async Task ClearCartAsync(int userId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand("DELETE ci FROM CartItems ci INNER JOIN Cart c ON ci.CartId = c.CartId WHERE c.UserId = @UserId", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            await cmd.ExecuteNonQueryAsync();
        }

        private static async Task<int> EnsureCartAsync(SqlConnection conn, int userId)
        {
            var checkCmd = new SqlCommand("SELECT CartId FROM Cart WHERE UserId = @UserId", conn);
            checkCmd.Parameters.AddWithValue("@UserId", userId);
            var result = await checkCmd.ExecuteScalarAsync();
            if (result != null) return (int)result;

            var createCmd = new SqlCommand("INSERT INTO Cart (UserId) OUTPUT INSERTED.CartId VALUES (@UserId)", conn);
            createCmd.Parameters.AddWithValue("@UserId", userId);
            return (int)(await createCmd.ExecuteScalarAsync())!;
        }
    }
}
