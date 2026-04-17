using WebApi.Models;

namespace WebApi.Repository.Interface
{
    public interface IProductRepository
    {
        Task<ProductListResponse> GetProductsAsync(ProductListRequest request);
        Task<ProductModel?> GetProductByIdAsync(int productId);
        Task<List<ProductModel>> GetFeaturedProductsAsync();
        Task<int> CreateProductAsync(ProductModel product);
        Task<bool> UpdateProductAsync(ProductModel product);
        Task<bool> DeleteProductAsync(int productId);
        Task<bool> UpdateStockAsync(int productId, int quantity);
    }
}
