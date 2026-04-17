using WebApi.Models;

namespace WebApi.Repository.Interface
{
    public interface ICartRepository
    {
        Task<CartModel?> GetCartByUserIdAsync(int userId);
        Task AddToCartAsync(int userId, AddToCartRequest request);
        Task UpdateCartItemAsync(UpdateCartItemRequest request);
        Task RemoveCartItemAsync(int cartItemId);
        Task ClearCartAsync(int userId);
    }
}
