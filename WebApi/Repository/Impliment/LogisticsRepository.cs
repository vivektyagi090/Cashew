using System.Data;
using Microsoft.Data.SqlClient;
using WebApi.Data;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Repository.Impliment
{
    public class LogisticsRepository : ILogisticsRepository
    {
        private readonly Helper _dbHelper;

        public LogisticsRepository(Helper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<IEnumerable<LogisticsModel>> GetAllTrackingAsync()
        {
            string query = "SELECT * FROM Logistics ORDER BY UpdatedAt DESC";
            var dt = await _dbHelper.FillDataTableAsync(query, CommandType.Text, CancellationToken.None);
            var list = new List<LogisticsModel>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(MapToLogistics(row));
            }
            return list;
        }

        public async Task<LogisticsModel?> GetTrackingByOrderIdAsync(int orderId)
        {
            string query = "SELECT * FROM Logistics WHERE OrderId = @OrderId";
            var parameters = new[] { new SqlParameter("@OrderId", orderId) };
            var dt = await _dbHelper.FillDataTableAsync(query, CommandType.Text, CancellationToken.None, parameters);
            
            if (dt.Rows.Count == 0) return null;
            return MapToLogistics(dt.Rows[0]);
        }

        public async Task<bool> UpdateLogisticsAsync(LogisticsModel logistics)
        {
            var parameters = new[]
            {
                new SqlParameter("@OrderId", logistics.OrderId),
                new SqlParameter("@CarrierName", logistics.CarrierName ?? ""),
                new SqlParameter("@TrackingNumber", (object?)logistics.TrackingNumber ?? DBNull.Value),
                new SqlParameter("@Status", logistics.Status ?? ""),
                new SqlParameter("@CurrentLocation", (object?)logistics.CurrentLocation ?? DBNull.Value),
                new SqlParameter("@EstimatedDelivery", (object?)logistics.EstimatedDelivery ?? DBNull.Value)
            };

            string query = @"
                IF EXISTS (SELECT 1 FROM Logistics WHERE OrderId = @OrderId)
                BEGIN
                    UPDATE Logistics SET 
                        CarrierName = @CarrierName, 
                        TrackingNumber = @TrackingNumber, 
                        Status = @Status, 
                        CurrentLocation = @CurrentLocation, 
                        EstimatedDelivery = @EstimatedDelivery,
                        UpdatedAt = GETDATE()
                    WHERE OrderId = @OrderId
                END
                ELSE
                BEGIN
                    INSERT INTO Logistics (OrderId, CarrierName, TrackingNumber, Status, CurrentLocation, EstimatedDelivery)
                    VALUES (@OrderId, @CarrierName, @TrackingNumber, @Status, @CurrentLocation, @EstimatedDelivery)
                END
                
                -- Update order status too
                UPDATE Orders SET Status = @Status WHERE OrderId = @OrderId";

            int result = await _dbHelper.ExecuteNonQueryAsync(query, null, CancellationToken.None, parameters);
            return result > 0;
        }

        private LogisticsModel MapToLogistics(DataRow row)
        {
            return new LogisticsModel
            {
                TrackingId = (int)row["TrackingId"],
                OrderId = (int)row["OrderId"],
                CarrierName = row["CarrierName"].ToString() ?? "",
                TrackingNumber = row["TrackingNumber"].ToString() ?? "",
                Status = row["Status"].ToString() ?? "",
                CurrentLocation = row["CurrentLocation"]?.ToString(),
                EstimatedDelivery = row["EstimatedDelivery"] != DBNull.Value ? (DateTime)row["EstimatedDelivery"] : null,
                UpdatedAt = (DateTime)row["UpdatedAt"]
            };
        }
    }
}
