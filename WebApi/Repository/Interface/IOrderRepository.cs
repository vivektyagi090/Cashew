using WebApi.Models;

namespace WebApi.Repository.Interface
{
    public interface IOrderRepository
    {
        Task<List<OrderModel>> GetOrdersByUserIdAsync(int userId);
        Task<OrderModel?> GetOrderByIdAsync(int orderId);
        Task<List<OrderModel>> GetAllOrdersAsync();
        Task<int> PlaceOrderAsync(int userId, PlaceOrderRequest request);
        Task<bool> UpdateOrderStatusAsync(UpdateOrderStatusRequest request);
    }
}
