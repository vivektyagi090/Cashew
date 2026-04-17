using System.Data;
using Microsoft.Data.SqlClient;
using WebApi.Data;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Repository.Impliment
{
    public class InventoryRepository : IInventoryRepository
    {
        private readonly Helper _dbHelper;

        public InventoryRepository(Helper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<IEnumerable<InventoryModel>> GetMovementsAsync()
        {
            string query = @"
                SELECT m.*, p.Name as ProductName 
                FROM InventoryMovements m
                JOIN Products p ON m.ProductId = p.ProductId
                ORDER BY m.CreatedAt DESC";
            
            var dt = await _dbHelper.FillDataTableAsync(query, CommandType.Text, CancellationToken.None);
            var movements = new List<InventoryModel>();

            foreach (DataRow row in dt.Rows)
            {
                movements.Add(new InventoryModel
                {
                    MovementId = (int)row["MovementId"],
                    ProductId = (int)row["ProductId"],
                    ProductName = row["ProductName"].ToString() ?? "",
                    Quantity = (int)row["Quantity"],
                    Type = row["Type"]?.ToString() ?? "",
                    ReferenceId = row["ReferenceId"] != DBNull.Value ? (int)row["ReferenceId"] : (int?)null,
                    Remarks = row["Remarks"]?.ToString() ?? "",
                    CreatedAt = (DateTime)row["CreatedAt"]
                });
            }
            return movements;
        }

        public async Task<bool> AddMovementAsync(InventoryModel movement)
        {
            // Transactional update: Add movement and update product stock
            var parameters = new[]
            {
                new SqlParameter("@ProductId", movement.ProductId),
                new SqlParameter("@Quantity", movement.Quantity),
                new SqlParameter("@Type", movement.Type),
                new SqlParameter("@ReferenceId", (object?)movement.ReferenceId ?? DBNull.Value),
                new SqlParameter("@Remarks", (object?)movement.Remarks ?? DBNull.Value)
            };

            string query = @"
                BEGIN TRANSACTION;
                INSERT INTO InventoryMovements (ProductId, Quantity, Type, ReferenceId, Remarks)
                VALUES (@ProductId, @Quantity, @Type, @ReferenceId, @Remarks);
                
                UPDATE Products SET StockQty = StockQty + @Quantity 
                WHERE ProductId = @ProductId;
                COMMIT TRANSACTION;";

            int result = await _dbHelper.ExecuteNonQueryAsync(query, null, CancellationToken.None, parameters);
            return result > 0;
        }

        public async Task<IEnumerable<DamageModel>> GetDamagesAsync()
        {
            string query = @"
                SELECT d.*, p.Name as ProductName, u.FullName as AdminName
                FROM Damages d
                JOIN Products p ON d.ProductId = p.ProductId
                JOIN Users u ON d.LoggedInBy = u.Id
                ORDER BY d.CreatedAt DESC";
            
            var dt = await _dbHelper.FillDataTableAsync(query, CommandType.Text, CancellationToken.None);
            var damages = new List<DamageModel>();

            foreach (DataRow row in dt.Rows)
            {
                damages.Add(new DamageModel
                {
                    DamageId = (int)row["DamageId"],
                    ProductId = (int)row["ProductId"],
                    ProductName = row["ProductName"].ToString() ?? "",
                    Quantity = (int)row["Quantity"],
                    Reason = row["Reason"].ToString() ?? "",
                    LossAmount = (decimal)row["LossAmount"],
                    LoggedInBy = (int)row["LoggedInBy"],
                    AdminName = row["AdminName"].ToString() ?? "",
                    CreatedAt = (DateTime)row["CreatedAt"]
                });
            }
            return damages;
        }

        public async Task<bool> LogDamageAsync(DamageModel damage)
        {
            var parameters = new[]
            {
                new SqlParameter("@ProductId", damage.ProductId),
                new SqlParameter("@Quantity", damage.Quantity),
                new SqlParameter("@Reason", damage.Reason),
                new SqlParameter("@LossAmount", damage.LossAmount),
                new SqlParameter("@LoggedInBy", damage.LoggedInBy)
            };

            string query = @"
                BEGIN TRANSACTION;
                INSERT INTO Damages (ProductId, Quantity, Reason, LossAmount, LoggedInBy)
                VALUES (@ProductId, @Quantity, @Reason, @LossAmount, @LoggedInBy);
                
                -- Damages subtract from stock
                UPDATE Products SET StockQty = StockQty - @Quantity 
                WHERE ProductId = @ProductId;

                INSERT INTO InventoryMovements (ProductId, Quantity, Type, Remarks)
                VALUES (@ProductId, -@Quantity, 'Damage', @Reason);
                COMMIT TRANSACTION;";

            int result = await _dbHelper.ExecuteNonQueryAsync(query, null, CancellationToken.None, parameters);
            return result > 0;
        }

        public async Task<IEnumerable<ReturnModel>> GetReturnsAsync()
        {
            string query = @"
                SELECT r.*, p.Name as ProductName
                FROM Returns r
                JOIN Products p ON r.ProductId = p.ProductId
                ORDER BY r.CreatedAt DESC";
            
            var dt = await _dbHelper.FillDataTableAsync(query, CommandType.Text, CancellationToken.None);
            var returnsList = new List<ReturnModel>();

            foreach (DataRow row in dt.Rows)
            {
                returnsList.Add(new ReturnModel
                {
                    ReturnId = (int)row["ReturnId"],
                    OrderId = (int)row["OrderId"],
                    ProductId = (int)row["ProductId"],
                    ProductName = row["ProductName"].ToString() ?? "",
                    Quantity = (int)row["Quantity"],
                    Reason = row["Reason"].ToString() ?? "",
                    Status = row["Status"].ToString() ?? "",
                    RefundAmount = (decimal)row["RefundAmount"],
                    CreatedAt = (DateTime)row["CreatedAt"]
                });
            }
            return returnsList;
        }

        public async Task<bool> ProcessReturnAsync(ReturnModel returnItem)
        {
            var parameters = new[]
            {
                new SqlParameter("@ReturnId", returnItem.ReturnId),
                new SqlParameter("@Status", returnItem.Status),
                new SqlParameter("@ProductId", returnItem.ProductId),
                new SqlParameter("@Quantity", returnItem.Quantity)
            };

            // If approved, put back in stock
            string query = @"
                UPDATE Returns SET Status = @Status WHERE ReturnId = @ReturnId;
                
                IF @Status = 'Approved'
                BEGIN
                    UPDATE Products SET StockQty = StockQty + @Quantity WHERE ProductId = @ProductId;
                    INSERT INTO InventoryMovements (ProductId, Quantity, Type, Remarks)
                    VALUES (@ProductId, @Quantity, 'Return', 'Customer Return Approved');
                END";

            int result = await _dbHelper.ExecuteNonQueryAsync(query, null, CancellationToken.None, parameters);
            return result > 0;
        }
    }
}
