namespace WebApi.Models
{
    public class InventoryModel
    {
        public int MovementId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Type { get; set; } = string.Empty; // Purchase, Sale, Return, Damage, Adjustment
        public int? ReferenceId { get; set; }
        public string? Remarks { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class DamageModel
    {
        public int DamageId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public decimal LossAmount { get; set; }
        public int LoggedInBy { get; set; }
        public string AdminName { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class ReturnModel
    {
        public int ReturnId { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public string Reason { get; set; } = string.Empty;
        public string Status { get; set; } = "Pending";
        public decimal RefundAmount { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class LogisticsModel
    {
        public int TrackingId { get; set; }
        public int OrderId { get; set; }
        public string CarrierName { get; set; } = string.Empty;
        public string TrackingNumber { get; set; } = string.Empty;
        public string Status { get; set; } = "Packed";
        public string? CurrentLocation { get; set; }
        public DateTime? EstimatedDelivery { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AdminStatsModel
    {
        public decimal TotalRevenue { get; set; }
        public decimal TotalProfit { get; set; }
        public int TotalOrders { get; set; }
        public int PendingDeliveries { get; set; }
        public int LowStockAlerts { get; set; }
        public List<RecentOrderDto> RecentOrders { get; set; } = new();
    }

    public class RecentOrderDto
    {
        public int OrderId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime Date { get; set; }
    }
}
