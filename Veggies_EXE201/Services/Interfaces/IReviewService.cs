using Veggies_EXE201.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Veggies_EXE201.Services
{
    public interface IReviewService
    {
      
        Task<IEnumerable<Review>> GetAllReviewsAsync();
        Task<Review?> GetReviewByIdAsync(int id);
        Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId);
        Task CreateReviewAsync(Review review);
        Task<bool> DeleteReviewAsync(int id);
    }
}