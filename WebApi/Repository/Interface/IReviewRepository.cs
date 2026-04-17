using System.Collections.Generic;
using System.Threading.Tasks;
using WebApi.Models;

namespace WebApi.Repository.Interface
{
    public interface IReviewRepository
    {
        Task<IEnumerable<ReviewModel>> GetProductReviewsAsync(int productId);
        Task<bool> AddReviewAsync(ReviewModel review);
        Task<bool> DeleteReviewAsync(int reviewId);
    }
}
