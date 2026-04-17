using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Models;

namespace WebApi.Repository.Interface
{
    public interface IWishlistRepository
    {
        Task<IEnumerable<WishlistModel>> GetUserWishlistAsync(int userId);
        Task<bool> AddToWishlistAsync(WishlistModel wishlist);
        Task<bool> RemoveFromWishlistAsync(int wishlistId);
        Task<bool> IsInWishlistAsync(int userId, int productId);
    }
}
