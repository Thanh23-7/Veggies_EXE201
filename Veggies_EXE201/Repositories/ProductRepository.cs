using Microsoft.EntityFrameworkCore;
using Veggies_EXE201.Models;

namespace Veggies_EXE201.Repositories
{
    public class ProductRepository : IProductRepository
    {
        private readonly VeggiesDb2Context _context;

        public ProductRepository(VeggiesDb2Context context)
        {
            _context = context;
        }

        // --- CÁC HÀM CŨ ---
        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.Include(p => p.Category).ToListAsync();
        }

        public async Task<IEnumerable<Category>> GetCategoriesAsync()
        {
            return await _context.Categories.ToListAsync();
        }

        // --- TRIỂN KHAI CÁC HÀM MỚI ---

        public async Task<Product?> GetByIdAsync(int id)
        {
            // Tìm sản phẩm theo ID, bao gồm cả Category liên quan
            return await _context.Products
                                 .Include(p => p.Category)
                                 .FirstOrDefaultAsync(p => p.ProductId == id);
        }

        public async Task AddAsync(Product product)
        {
            // Thêm sản phẩm mới vào DbContext
            _context.Products.Add(product);
            // Lưu thay đổi vào database
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Product product)
        {
            // Đánh dấu sản phẩm là đã bị thay đổi
            _context.Entry(product).State = EntityState.Modified;
            // Lưu thay đổi vào database
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Product product)
        {
            // Xóa sản phẩm khỏi DbContext
            _context.Products.Remove(product);
            // Lưu thay đổi vào database
            await _context.SaveChangesAsync();
        }
    }
}