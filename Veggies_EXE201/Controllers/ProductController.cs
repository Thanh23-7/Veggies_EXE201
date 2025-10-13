using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veggies_EXE201.Models;
using Veggies_EXE201.Models.ViewModels; // <-- Đảm bảo namespace ViewModel là đúng
using System;
using System.Linq;
using System.Threading.Tasks;

public class ProductController : Controller
{
    private readonly VeggiesDb2Context _context;

    public ProductController(VeggiesDb2Context context)
    {
        _context = context;
    }

    // GET: /Product/Index
    public async Task<IActionResult> Index(ProductListViewModel request) // Nhận toàn bộ ViewModel làm tham số
    {
        int pageSize = 9; // Hoặc bạn có thể lấy từ request.PageSize nếu muốn

        // 1. Bắt đầu truy vấn
        var query = _context.Products.Include(p => p.Category).AsQueryable();

        // 2. Áp dụng bộ lọc
        if (!string.IsNullOrEmpty(request.SearchTerm))
        {
            query = query.Where(p => p.ProductName.Contains(request.SearchTerm));
        }
        if (request.CategoryId.HasValue)
        {
            query = query.Where(p => p.CategoryId == request.CategoryId.Value);
        }
        if (request.MinPrice.HasValue)
        {
            query = query.Where(p => p.Price >= request.MinPrice.Value);
        }
        if (request.MaxPrice.HasValue)
        {
            query = query.Where(p => p.Price <= request.MaxPrice.Value);
        }

        // 3. Áp dụng sắp xếp
        query = request.CurrentSort switch
        {
            "name_desc" => query.OrderByDescending(p => p.ProductName),
            "price_asc" => query.OrderBy(p => p.Price),
            "price_desc" => query.OrderByDescending(p => p.Price),
            _ => query.OrderBy(p => p.ProductName), // Mặc định
        };

        // 4. Lấy tổng số sản phẩm
        var totalItems = await query.CountAsync();

        // 5. Áp dụng phân trang và chuyển đổi sang ProductItem
        var productsForPage = await query
            .Skip((request.CurrentPage > 0 ? request.CurrentPage - 1 : 0) * pageSize)
            .Take(pageSize)
            .Select(p => new ProductListViewModel.ProductItem // <-- Tạo đối tượng ProductItem
            {
                ProductId = p.ProductId,
                ProductName = p.ProductName,
                ProductImage = p.ProductImage,
                Price = p.Price,
                CategoryName = p.Category != null ? p.Category.CategoryName : "Chưa phân loại"
            })
            .ToListAsync();

        // Lấy danh sách danh mục cho sidebar
        var allCategories = await _context.Categories
            .Select(c => new ProductListViewModel.CategoryItem // <-- Tạo đối tượng CategoryItem
            {
                CategoryId = c.CategoryId,
                CategoryName = c.CategoryName
            })
            .ToListAsync();

        // 6. Tạo ViewModel cuối cùng để gửi đến View
        var viewModel = new ProductListViewModel
        {
            Products = productsForPage,
            Categories = allCategories,
            TotalItems = totalItems,
            CurrentPage = request.CurrentPage > 0 ? request.CurrentPage : 1,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalItems / (double)pageSize),
            SearchTerm = request.SearchTerm,
            CategoryId = request.CategoryId,
            MinPrice = request.MinPrice,
            MaxPrice = request.MaxPrice,
            CurrentSort = request.CurrentSort
        };

        return View(viewModel);
    }
    // ... các action khác như Index() ...

    // GET: /Product/Details/5
    public async Task<IActionResult> Details(int id)
    {
        var product = await _context.Products
            .Include(p => p.Category) // Lấy cả thông tin danh mục
            .FirstOrDefaultAsync(m => m.ProductId == id);

        if (product == null)
        {
            return NotFound(); // Trả về 404 nếu không tìm thấy sản phẩm
        }

        // Bạn sẽ cần tạo một View tên là "Details.cshtml" trong thư mục "Views/Product"
        return View(product);
    }

    // ... các action khác ...
}