using System.Threading.Tasks;
using Veggies_EXE201.Models;
using Veggies_EXE201.Models.ViewModels;
using System.Collections.Generic;

namespace Veggies_EXE201.Services
{
    public interface IProductService
    {
        // Hàm chính cho trang danh sách sản phẩm
        Task<ProductListViewModel> GetProductsViewModelAsync(string? searchTerm, int? categoryId, decimal? minPrice, decimal? maxPrice, string sortBy, int pageNumber);

        // === CÁC HÀM CRUD MỚI ===
        Task<IEnumerable<Product>> GetAllProductsAsync();
        Task<Product?> GetProductByIdAsync(int productId);
        Task<IEnumerable<Category>> GetAllCategoriesAsync();
        Task CreateProductAsync(Product product);
        Task UpdateProductAsync(Product product);
        Task DeleteProductAsync(int productId);
    }
}