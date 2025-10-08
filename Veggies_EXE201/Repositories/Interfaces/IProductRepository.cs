using Veggies_EXE201.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Veggies_EXE201.Repositories
{
    public interface IProductRepository
    {
        // === CÁC PHƯƠNG THỨC CŨ ===
        Task<IEnumerable<Product>> GetAllAsync();
        Task<IEnumerable<Category>> GetCategoriesAsync();

        // === THÊM CÁC PHƯƠNG THỨC MỚI ===
        Task<Product?> GetByIdAsync(int id);
        Task AddAsync(Product product);
        Task UpdateAsync(Product product);
        Task DeleteAsync(Product product);
    }
}