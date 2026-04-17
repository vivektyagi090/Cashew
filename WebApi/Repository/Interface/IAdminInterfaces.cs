using WebApi.Models;

namespace WebApi.Repository.Interface
{
    public interface IInventoryRepository
    {
        Task<IEnumerable<InventoryModel>> GetMovementsAsync();
        Task<bool> AddMovementAsync(InventoryModel movement);
        Task<IEnumerable<DamageModel>> GetDamagesAsync();
        Task<bool> LogDamageAsync(DamageModel damage);
        Task<IEnumerable<ReturnModel>> GetReturnsAsync();
        Task<bool> ProcessReturnAsync(ReturnModel returnItem);
    }

    public interface ILogisticsRepository
    {
        Task<IEnumerable<LogisticsModel>> GetAllTrackingAsync();
        Task<LogisticsModel?> GetTrackingByOrderIdAsync(int orderId);
        Task<bool> UpdateLogisticsAsync(LogisticsModel logistics);
    }

    public interface IAdminRepository
    {
        Task<AdminStatsModel> GetDashboardStatsAsync();
        Task<IEnumerable<ProductModel>> GetInventoryMasterAsync();
    }
}
