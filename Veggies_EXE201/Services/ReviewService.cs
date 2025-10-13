using Microsoft.EntityFrameworkCore;
using Veggies_EXE201.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Veggies_EXE201.Services
{
    public class ReviewService : IReviewService
    {
        private readonly VeggiesDb2Context _context;
        public ReviewService(VeggiesDb2Context context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Review>> GetAllReviewsAsync()
        {
            return await _context.Reviews
                                 .Include(r => r.User) 
                                 .Include(r => r.Product) 
                                 .ToListAsync();
        }

        public async Task<Review?> GetReviewByIdAsync(int id)
        {
            return await _context.Reviews.FindAsync(id);
        }

        public async Task<IEnumerable<Review>> GetReviewsByProductIdAsync(int productId)
        {
            return await _context.Reviews
                                 .Where(r => r.ProductId == productId)
                                 .Include(r => r.User)
                                 .ToListAsync();
        }

        public async Task CreateReviewAsync(Review review)
        {
            _context.Reviews.Add(review);    
            await _context.SaveChangesAsync();
        }

        public async Task<bool> DeleteReviewAsync(int id)
        {
            var reviewToDelete = await _context.Reviews.FindAsync(id);
            if (reviewToDelete == null)
            {
                return false;
            }

            _context.Reviews.Remove(reviewToDelete);
            await _context.SaveChangesAsync();
            return true;
        }
    }
}