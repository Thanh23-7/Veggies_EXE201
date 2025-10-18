using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using Veggies_EXE201.Services;
using Veggies_EXE201.Models;
using Microsoft.EntityFrameworkCore;

namespace Veggies_EXE201.Controllers
{
    /// <summary>
    /// Controller này quản lý tất cả các chức năng của trang Admin.
    /// Yêu cầu người dùng phải đăng nhập và có vai trò "Admin".
    /// </summary>
    [Authorize(Roles = "Admin")]
    [Route("admin")]
    public class AdminController : Controller
    {
        private readonly AuthService _authService;
        private readonly VeggiesDb2Context _context;
        private readonly ProductService _productService;
        private readonly ReviewService _reviewService;

        public AdminController(
            AuthService authService,
            VeggiesDb2Context context,
            ProductService productService,
            ReviewService reviewService)
        {
            _authService = authService;
            _context = context;
            _productService = productService;
            _reviewService = reviewService;
        }

        //--- Trang Dashboard chính của Admin ---
        [HttpGet]
        [HttpGet("index")]
        public async Task<IActionResult> Index()
        {
            // Lấy tên của admin đang đăng nhập
            string adminName = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
            ViewData["AdminName"] = adminName;

            // Thống kê tổng quan
            var totalUsers = await _context.Users.CountAsync();
            var totalProducts = await _context.Products.CountAsync();
            var totalOrders = await _context.Orders.CountAsync();
            var totalReviews = await _context.Reviews.CountAsync();

            // Thống kê theo tháng (7 tháng gần nhất)
            var monthlyStats = await GetMonthlyStatistics();

            ViewData["TotalUsers"] = totalUsers;
            ViewData["TotalProducts"] = totalProducts;
            ViewData["TotalOrders"] = totalOrders;
            ViewData["TotalReviews"] = totalReviews;
            ViewData["MonthlyStats"] = monthlyStats;

            return View();
        }

        //--- Quản lý Tài khoản (Users) ---
        [HttpGet("accounts")]
        public async Task<IActionResult> ManageAccounts()
        {
            ViewData["Title"] = "Quản lý Tài khoản";
            var allUsers = await _authService.GetAllUsersAsync();
            return View("ManageAccounts", allUsers);
        }

        [HttpPost("accounts/toggle/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleUserStatus(int id)
        {
            // Tạm thời chưa có trường IsActive, sẽ implement sau
            TempData["SuccessMessage"] = "Chức năng đang được phát triển";
            return RedirectToAction(nameof(ManageAccounts));
        }

        //--- Quản lý Sản phẩm (Products) ---
        [HttpGet("products")]
        public async Task<IActionResult> ManageProducts()
        {
            ViewData["Title"] = "Quản lý Sản phẩm";
            var allProducts = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();
            return View("Products/Index", allProducts);
        }

        [HttpGet("products/create")]
        public async Task<IActionResult> CreateProduct()
        {
            ViewData["Title"] = "Thêm Sản phẩm mới";
            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = categories;
            return View("Products/Create");
        }

        [HttpPost("products/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product newProduct)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    _context.Products.Add(newProduct);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã thêm sản phẩm thành công!";
                    return RedirectToAction(nameof(ManageProducts));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi thêm sản phẩm: " + ex.Message;
                }
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = categories;
            return View("Products/Create", newProduct);
        }

        [HttpGet("products/edit/{id}")]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id);

            if (product == null)
            {
                return NotFound();
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = categories;
            return View("Products/Edit", product);
        }

        [HttpPost("products/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(product);
                    await _context.SaveChangesAsync();
                    TempData["SuccessMessage"] = "Đã cập nhật sản phẩm thành công!";
                    return RedirectToAction(nameof(ManageProducts));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi cập nhật sản phẩm: " + ex.Message;
                }
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = categories;
            return View("Products/Edit", product);
        }

        [HttpPost("products/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product != null)
            {
                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa sản phẩm thành công!";
            }
            return RedirectToAction(nameof(ManageProducts));
        }

        //--- Quản lý Đánh giá (Reviews) ---
        [HttpGet("reviews")]
        public async Task<IActionResult> ManageReviews()
        {
            ViewData["Title"] = "Quản lý Đánh giá";
            var allReviews = await _context.Reviews
                .Include(r => r.User)
                .Include(r => r.Product)
                .ToListAsync();
            return View("Reviews/Index", allReviews);
        }

        [HttpPost("reviews/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteReview(int id)
        {
            var review = await _context.Reviews.FindAsync(id);
            if (review != null)
            {
                _context.Reviews.Remove(review);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa đánh giá thành công!";
            }
            return RedirectToAction(nameof(ManageReviews));
        }
        private async Task<Dictionary<string, object>> GetMonthlyStatistics()
        {
            var endDate = DateTime.Now;
            // Lấy dữ liệu 7 tháng gần nhất
            var startDate = endDate.AddMonths(-6);

            var monthlyData = new Dictionary<string, object>();

            // 1. Thống kê đơn hàng theo tháng (SỬA LỖI LINQ TẠI ĐÂY)
            var rawMonthlyOrders = await _context.Orders
                .Where(o => o.OrderDate.HasValue && o.OrderDate >= startDate)
                .GroupBy(o => new { o.OrderDate.Value.Year, o.OrderDate.Value.Month })
                .Select(g => new
                {
                    // Chỉ lấy Year và Month (số) từ DB, không định dạng chuỗi
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count(),
                    Total = g.Sum(o => o.TotalAmount ?? 0)
                })
                .ToListAsync(); // Tải dữ liệu thô về bộ nhớ

            // Xử lý định dạng chuỗi và sắp xếp trên bộ nhớ (Client)
            var monthlyOrders = rawMonthlyOrders
                .Select(x => new
                {
                    // Định dạng chuỗi Month-Year trên Client
                    Month = x.Year + "-" + x.Month.ToString("D2"),
                    x.Count,
                    x.Total
                })
                .OrderBy(x => x.Month)
                .ToList();


            // 2. Thống kê người dùng mới theo tháng (SỬA LỖI LINQ TẠI ĐÂY)
            var rawMonthlyUsers = await _context.Users
                .Where(u => u.CreatedAt.HasValue && u.CreatedAt.Value >= startDate)
                .GroupBy(u => new { u.CreatedAt.Value.Year, u.CreatedAt.Value.Month })
                .Select(g => new
                {
                    // Chỉ lấy Year và Month (số) từ DB, không định dạng chuỗi
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Count = g.Count()
                })
                .ToListAsync(); // Tải dữ liệu thô về bộ nhớ

            // Xử lý định dạng chuỗi và sắp xếp trên bộ nhớ (Client)
            var monthlyUsers = rawMonthlyUsers
                .Select(x => new
                {
                    // Định dạng chuỗi Month-Year trên Client
                    Month = x.Year + "-" + x.Month.ToString("D2"),
                    x.Count
                })
                .OrderBy(x => x.Month)
                .ToList();


            monthlyData["Orders"] = monthlyOrders;
            monthlyData["Users"] = monthlyUsers;

            return monthlyData;
        }
    }
}