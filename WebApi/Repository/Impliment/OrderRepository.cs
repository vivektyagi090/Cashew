using Microsoft.Data.SqlClient;
using WebApi.Data;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Repository.Impliment
{
    public class OrderRepository : IOrderRepository
    {
        private readonly DbConnectionFactory _db;
        private readonly ICartRepository _cart;

        public OrderRepository(DbConnectionFactory db, ICartRepository cart)
        {
            _db   = db;
            _cart = cart;
        }

        public async Task<List<OrderModel>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = new List<OrderModel>();
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();

            var cmd = new SqlCommand(@"SELECT OrderId, UserId, OrderDate, Status, TotalAmount,
                                       ShippingAddress, City, State, ZipCode, PaymentMethod, Notes
                                       FROM Orders WHERE UserId = @UserId ORDER BY OrderDate DESC", conn);
            cmd.Parameters.AddWithValue("@UserId", userId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                orders.Add(MapOrder(reader));
            reader.Close();

            foreach (var order in orders)
                order.Items = await GetOrderItemsAsync(conn, order.OrderId);

            return orders;
        }

        public async Task<OrderModel?> GetOrderByIdAsync(int orderId)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"SELECT OrderId, UserId, OrderDate, Status, TotalAmount,
                                       ShippingAddress, City, State, ZipCode, PaymentMethod, Notes
                                       FROM Orders WHERE OrderId = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", orderId);
            using var reader = await cmd.ExecuteReaderAsync();
            if (!await reader.ReadAsync()) return null;
            var order = MapOrder(reader);
            reader.Close();
            order.Items = await GetOrderItemsAsync(conn, orderId);
            return order;
        }

        public async Task<List<OrderModel>> GetAllOrdersAsync()
        {
            var orders = new List<OrderModel>();
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand(@"SELECT OrderId, UserId, OrderDate, Status, TotalAmount,
                                       ShippingAddress, City, State, ZipCode, PaymentMethod, Notes
                                       FROM Orders ORDER BY OrderDate DESC", conn);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                orders.Add(MapOrder(reader));
            return orders;
        }

        public async Task<int> PlaceOrderAsync(int userId, PlaceOrderRequest request)
        {
            var cart = await _cart.GetCartByUserIdAsync(userId);
            if (cart == null || !cart.Items.Any())
                throw new InvalidOperationException("Cart is empty.");

            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            using var tx = conn.BeginTransaction();
            try
            {
                // Create order
                var orderCmd = new SqlCommand(@"INSERT INTO Orders (UserId, TotalAmount, ShippingAddress, City, State, ZipCode, PaymentMethod, Notes)
                                                OUTPUT INSERTED.OrderId
                                                VALUES (@UserId, @Total, @Addr, @City, @State, @Zip, @Pay, @Notes)", conn, tx);
                orderCmd.Parameters.AddWithValue("@UserId", userId);
                orderCmd.Parameters.AddWithValue("@Total",  cart.TotalAmount);
                orderCmd.Parameters.AddWithValue("@Addr",   request.ShippingAddress);
                orderCmd.Parameters.AddWithValue("@City",   request.City);
                orderCmd.Parameters.AddWithValue("@State",  request.State);
                orderCmd.Parameters.AddWithValue("@Zip",    request.ZipCode);
                orderCmd.Parameters.AddWithValue("@Pay",    request.PaymentMethod);
                orderCmd.Parameters.AddWithValue("@Notes",  (object?)request.Notes ?? DBNull.Value);
                int orderId = (int)(await orderCmd.ExecuteScalarAsync())!;

                // Insert order items
                foreach (var item in cart.Items)
                {
                    var itemCmd = new SqlCommand(@"INSERT INTO OrderItems (OrderId, ProductId, ProductName, Quantity, UnitPrice)
                                                   VALUES (@OId, @PId, @PName, @Qty, @Price)", conn, tx);
                    itemCmd.Parameters.AddWithValue("@OId",   orderId);
                    itemCmd.Parameters.AddWithValue("@PId",   item.ProductId);
                    itemCmd.Parameters.AddWithValue("@PName", item.ProductName);
                    itemCmd.Parameters.AddWithValue("@Qty",   item.Quantity);
                    itemCmd.Parameters.AddWithValue("@Price", item.Price);
                    await itemCmd.ExecuteNonQueryAsync();

                    // Update stock
                    var stockCmd = new SqlCommand("UPDATE Products SET StockQty = StockQty - @Qty WHERE ProductId = @PId", conn, tx);
                    stockCmd.Parameters.AddWithValue("@Qty", item.Quantity);
                    stockCmd.Parameters.AddWithValue("@PId", item.ProductId);
                    await stockCmd.ExecuteNonQueryAsync();
                }

                // Clear cart
                var clearCmd = new SqlCommand("DELETE ci FROM CartItems ci INNER JOIN Cart c ON ci.CartId = c.CartId WHERE c.UserId = @UserId", conn, tx);
                clearCmd.Parameters.AddWithValue("@UserId", userId);
                await clearCmd.ExecuteNonQueryAsync();

                await tx.CommitAsync();
                return orderId;
            }
            catch
            {
                await tx.RollbackAsync();
                throw;
            }
        }

        public async Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusRequest request)
        {
            using var conn = _db.CreateConnection();
            await conn.OpenAsync();
            var cmd = new SqlCommand("UPDATE Orders SET Status = @Status WHERE OrderId = @Id", conn);
            cmd.Parameters.AddWithValue("@Status", request.Status);
            cmd.Parameters.AddWithValue("@Id",     request.OrderId);
            return await cmd.ExecuteNonQueryAsync() > 0;
        }

        private static async Task<List<OrderItemModel>> GetOrderItemsAsync(SqlConnection conn, int orderId)
        {
            var items = new List<OrderItemModel>();
            var cmd = new SqlCommand("SELECT OrderItemId, OrderId, ProductId, ProductName, Quantity, UnitPrice FROM OrderItems WHERE OrderId = @Id", conn);
            cmd.Parameters.AddWithValue("@Id", orderId);
            using var reader = await cmd.ExecuteReaderAsync();
            while (await reader.ReadAsync())
                items.Add(new OrderItemModel
                {
                    OrderItemId = reader.GetInt32(0),
                    OrderId     = reader.GetInt32(1),
                    ProductId   = reader.GetInt32(2),
                    ProductName = reader.GetString(3),
                    Quantity    = reader.GetInt32(4),
                    UnitPrice   = reader.GetDecimal(5)
                });
            return items;
        }

        private static OrderModel MapOrder(SqlDataReader r) => new()
        {
            OrderId         = r.GetInt32(0),
            UserId          = r.GetInt32(1),
            OrderDate       = r.GetDateTime(2),
            Status          = r.GetString(3),
            TotalAmount     = r.GetDecimal(4),
            ShippingAddress = r.IsDBNull(5) ? null : r.GetString(5),
            City            = r.IsDBNull(6) ? null : r.GetString(6),
            State           = r.IsDBNull(7) ? null : r.GetString(7),
            ZipCode         = r.IsDBNull(8) ? null : r.GetString(8),
            PaymentMethod   = r.IsDBNull(9) ? null : r.GetString(9),
            Notes           = r.IsDBNull(10) ? null : r.GetString(10)
        };
    }
}
