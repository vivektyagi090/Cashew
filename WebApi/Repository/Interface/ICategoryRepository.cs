using WebApi.Models;

namespace WebApi.Repository.Interface
{
    public interface ICategoryRepository
    {
        Task<List<CategoryModel>> GetAllCategoriesAsync();
        Task<CategoryModel?> GetCategoryByIdAsync(int categoryId);
        Task<int> CreateCategoryAsync(CategoryModel category);
        Task<bool> UpdateCategoryAsync(CategoryModel category);
        Task<bool> DeleteCategoryAsync(int categoryId);
    }
}
