using System.Data;
using Microsoft.Data.SqlClient;
using WebApi.Data;
using WebApi.Models;
using WebApi.Repository.Interface;

namespace WebApi.Repository.Impliment
{
    public class AdminRepository : IAdminRepository
    {
        private readonly Helper _dbHelper;

        public AdminRepository(Helper dbHelper)
        {
            _dbHelper = dbHelper;
        }

        public async Task<AdminStatsModel> GetDashboardStatsAsync()
        {
            // Aggregated stats query
            string query = @"
                SELECT 
                    (SELECT ISNULL(SUM(TotalAmount), 0) FROM Orders WHERE Status != 'Cancelled') as TotalRevenue,
                    (SELECT ISNULL(SUM(oi.Quantity * (oi.UnitPrice - p.CostPrice)), 0) 
                     FROM OrderItems oi 
                     JOIN Products p ON oi.ProductId = p.ProductId
                     JOIN Orders o ON oi.OrderId = o.OrderId
                     WHERE o.Status != 'Cancelled') as TotalProfit,
                    count(OrderId) as TotalOrders
                FROM Orders;
                
                SELECT count(*) FROM Logistics WHERE Status != 'Delivered';
                SELECT count(*) FROM Products WHERE StockQty < 10 AND IsActive = 1;
                
                SELECT TOP 5 o.OrderId, u.FullName as CustomerName, o.TotalAmount as Amount, o.Status, o.CreatedAt as Date
                FROM Orders o
                JOIN Users u ON o.UserId = u.Id
                ORDER BY o.CreatedAt DESC;";

            var ds = await _dbHelper.ExecuteQueryAsync(query, CommandType.Text, CancellationToken.None);
            
            var stats = new AdminStatsModel();
            if (ds != null && ds.Tables.Count > 0)
            {
                var mainRow = ds.Tables[0].Rows[0];
                stats.TotalRevenue = mainRow["TotalRevenue"] != DBNull.Value ? (decimal)mainRow["TotalRevenue"] : 0;
                stats.TotalProfit = mainRow["TotalProfit"] != DBNull.Value ? (decimal)mainRow["TotalProfit"] : 0;
                stats.TotalOrders = (int)mainRow["TotalOrders"];
                
                stats.PendingDeliveries = (int)ds.Tables[1].Rows[0][0];
                stats.LowStockAlerts = (int)ds.Tables[2].Rows[0][0];
                
                foreach (DataRow row in ds.Tables[3].Rows)
                {
                    stats.RecentOrders.Add(new RecentOrderDto
                    {
                        OrderId = (int)row["OrderId"],
                        CustomerName = row["CustomerName"]?.ToString() ?? string.Empty,
                        Amount = (decimal)row["Amount"],
                        Status = row["Status"]?.ToString() ?? string.Empty,
                        Date = (DateTime)row["Date"]
                    });
                }
            }
            return stats;
        }

        public async Task<IEnumerable<ProductModel>> GetInventoryMasterAsync()
        {
            string query = "SELECT p.*, c.Name as CategoryName FROM Products p JOIN Categories c ON p.CategoryId = c.CategoryId WHERE p.IsActive = 1";
            var dt = await _dbHelper.FillDataTableAsync(query, CommandType.Text, CancellationToken.None);
            var list = new List<ProductModel>();

            foreach (DataRow row in dt.Rows)
            {
                list.Add(new ProductModel
                {
                    ProductId = (int)row["ProductId"],
                    CategoryId = (int)row["CategoryId"],
                    CategoryName = row["CategoryName"].ToString() ?? "",
                    Name = row["Name"].ToString() ?? "",
                    Description = row["Description"]?.ToString(),
                    Price = (decimal)row["Price"],
                    CostPrice = row["CostPrice"] != DBNull.Value ? (decimal)row["CostPrice"] : 0,
                    OriginalPrice = row["OriginalPrice"] != DBNull.Value ? (decimal)row["OriginalPrice"] : null,
                    StockQty = (int)row["StockQty"],
                    ImageUrl = row["ImageUrl"]?.ToString(),
                    Rating = (decimal)row["Rating"],
                    ReviewCount = (int)row["ReviewCount"],
                    Brand = row["Brand"]?.ToString(),
                    IsActive = (bool)row["IsActive"],
                    IsFeatured = (bool)row["IsFeatured"],
                    CreatedAt = (DateTime)row["CreatedAt"]
                });
            }
            return list;
        }
    }
}
