using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Veggies_EXE201.Models;
using Veggies_EXE201.Services;
using Microsoft.AspNetCore.Hosting;
using System.IO;

namespace Veggies_EXE201.Controllers
{
    /// <summary>
    /// Controller quản lý các chức năng dành cho người bán (Seller)
    /// Yêu cầu người dùng phải đăng nhập và có vai trò "Seller"
    /// </summary>
    [Authorize(Roles = "Seller")]
    [Route("seller")]
    public class SellerController : Controller
    {
        private readonly VeggiesDb2Context _context;
        private readonly ProductService _productService;
        private readonly ActivityLogService _activityLogService;
        private readonly IWebHostEnvironment _env;

        public SellerController(
            VeggiesDb2Context context,
            ProductService productService,
            ActivityLogService activityLogService,
            IWebHostEnvironment env)
        {
            _context = context;
            _productService = productService;
            _activityLogService = activityLogService;
            _env = env;
        }

        //--- Trang Dashboard chính của Seller ---
        [HttpGet]
        [HttpGet("index")]
        public async Task<IActionResult> Index()
        {
            var sellerId = GetCurrentUserId();
            var sellerName = User.FindFirstValue(ClaimTypes.Name) ?? "Seller";

            ViewData["SellerName"] = sellerName;

            // Thống kê tổng quan cho seller
            var totalProducts = await _context.Products
                .Where(p => p.SellerId == sellerId)
                .CountAsync();

            var totalOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.OrderDetails.Any(od => od.Product.SellerId == sellerId))
                .CountAsync();

            var totalRevenue = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.OrderDetails.Any(od => od.Product.SellerId == sellerId))
                .SumAsync(o => o.TotalAmount ?? 0);

            var pendingOrders = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.Status == "Pending" && o.OrderDetails.Any(od => od.Product.SellerId == sellerId))
                .CountAsync();

            ViewData["TotalProducts"] = totalProducts;
            ViewData["TotalOrders"] = totalOrders;
            ViewData["TotalRevenue"] = totalRevenue;
            ViewData["PendingOrders"] = pendingOrders;

            // Lấy sản phẩm bán chạy nhất
            var topProducts = await _context.Products
                .Where(p => p.SellerId == sellerId)
                .Include(p => p.OrderDetails)
                .OrderByDescending(p => p.OrderDetails.Sum(od => od.Quantity))
                .Take(5)
                .ToListAsync();

            ViewData["TopProducts"] = topProducts;

            return View();
        }

        //--- Quản lý Sản phẩm của Seller ---
        [HttpGet("products")]
        public async Task<IActionResult> ManageProducts()
        {
            var sellerId = GetCurrentUserId();
            ViewData["Title"] = "Quản lý Sản phẩm";

            var products = await _context.Products
                .Where(p => p.SellerId == sellerId)
                .Include(p => p.Category)
                .Include(p => p.Reviews)
                .ToListAsync();

            return View(products);
        }

        [HttpGet("products/create")]
        public async Task<IActionResult> CreateProduct()
        {
            ViewData["Title"] = "Thêm Sản phẩm mới";
            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = new SelectList(categories, "CategoryId", "CategoryName");
            return View();
        }

        [HttpPost("products/create")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product newProduct, IFormFile? productImage)
        {
            if (ModelState.IsValid)
            {
                try
                {
                    newProduct.SellerId = GetCurrentUserId();
                    newProduct.CreatedAt = DateTime.Now;
                    // Sanitize long/base64 URLs
                    newProduct.VietGapCertificateUrl = SanitizeUrlString(newProduct.VietGapCertificateUrl);
                    newProduct.CultivationVideoUrl = SanitizeUrlString(newProduct.CultivationVideoUrl);

                    // Handle image upload
                    if (productImage != null && productImage.Length > 0)
                    {
                        var uploadsDir = Path.Combine(_env.WebRootPath, "images", "products");
                        Directory.CreateDirectory(uploadsDir);
                        var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(productImage.FileName);
                        var savePath = Path.Combine(uploadsDir, fileName);
                        using (var stream = new FileStream(savePath, FileMode.Create))
                        {
                            await productImage.CopyToAsync(stream);
                        }
                        newProduct.ProductImage = "/images/products/" + fileName;
                    }
                    
                    _context.Products.Add(newProduct);
                    await _context.SaveChangesAsync();

                    // Log activity
                    await _activityLogService.LogActivityAsync(
                        GetCurrentUserId(),
                        "CREATE_PRODUCT",
                        $"Đã tạo sản phẩm mới: {newProduct.ProductName}",
                        HttpContext.Connection.RemoteIpAddress?.ToString()
                    );

                    TempData["SuccessMessage"] = "Đã thêm sản phẩm thành công!";
                    return RedirectToAction(nameof(ManageProducts));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi thêm sản phẩm: " + ex.Message;
                }
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = new SelectList(categories, "CategoryId", "CategoryName");
            return View(newProduct);
        }

        [HttpGet("products/edit/{id}")]
        public async Task<IActionResult> EditProduct(int id)
        {
            var sellerId = GetCurrentUserId();
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.ProductId == id && p.SellerId == sellerId);

            if (product == null)
            {
                return NotFound();
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = new SelectList(categories, "CategoryId", "CategoryName");
            return View(product);
        }

        [HttpPost("products/edit/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product product, IFormFile? productImage)
        {
            if (id != product.ProductId)
            {
                return NotFound();
            }

            var sellerId = GetCurrentUserId();
            var existingProduct = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && p.SellerId == sellerId);

            if (existingProduct == null)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    // Cập nhật thông tin sản phẩm
                    existingProduct.ProductName = product.ProductName;
                    existingProduct.Description = product.Description;
                    existingProduct.Price = product.Price;
                    existingProduct.Stock = product.Stock;
                    existingProduct.CategoryId = product.CategoryId;
                    // Sanitize long/base64 URLs
                    existingProduct.VietGapCertificateUrl = SanitizeUrlString(product.VietGapCertificateUrl);
                    existingProduct.CultivationVideoUrl = SanitizeUrlString(product.CultivationVideoUrl);
                    // Handle image replace
                    if (productImage != null && productImage.Length > 0)
                    {
                        var uploadsDir = Path.Combine(_env.WebRootPath, "images", "products");
                        Directory.CreateDirectory(uploadsDir);
                        var fileName = Guid.NewGuid().ToString("N") + Path.GetExtension(productImage.FileName);
                        var savePath = Path.Combine(uploadsDir, fileName);
                        using (var stream = new FileStream(savePath, FileMode.Create))
                        {
                            await productImage.CopyToAsync(stream);
                        }
                        // Delete old image if local
                        if (!string.IsNullOrWhiteSpace(existingProduct.ProductImage) && existingProduct.ProductImage.StartsWith("/images/"))
                        {
                            var oldPath = Path.Combine(_env.WebRootPath, existingProduct.ProductImage.TrimStart('/'));
                            if (System.IO.File.Exists(oldPath))
                            {
                                System.IO.File.Delete(oldPath);
                            }
                        }
                        existingProduct.ProductImage = "/images/products/" + fileName;
                    }
                    existingProduct.UpdatedAt = DateTime.Now;

                    _context.Update(existingProduct);
                    await _context.SaveChangesAsync();

                    // Log activity
                    await _activityLogService.LogActivityAsync(
                        GetCurrentUserId(),
                        "UPDATE_PRODUCT",
                        $"Đã cập nhật sản phẩm: {product.ProductName}",
                        HttpContext.Connection.RemoteIpAddress?.ToString()
                    );

                    TempData["SuccessMessage"] = "Đã cập nhật sản phẩm thành công!";
                    return RedirectToAction(nameof(ManageProducts));
                }
                catch (Exception ex)
                {
                    TempData["ErrorMessage"] = "Lỗi khi cập nhật sản phẩm: " + ex.Message;
                }
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = new SelectList(categories, "CategoryId", "CategoryName");
            return View(product);
        }

        [HttpPost("products/delete/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var sellerId = GetCurrentUserId();
            var product = await _context.Products
                .FirstOrDefaultAsync(p => p.ProductId == id && p.SellerId == sellerId);

            if (product != null)
            {
                // Log activity
                await _activityLogService.LogActivityAsync(
                    GetCurrentUserId(),
                    "DELETE_PRODUCT",
                    $"Đã xóa sản phẩm: {product.ProductName}",
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                _context.Products.Remove(product);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa sản phẩm thành công!";
            }
            return RedirectToAction(nameof(ManageProducts));
        }

        //--- Quản lý Đơn hàng của Seller ---
        [HttpGet("orders")]
        public async Task<IActionResult> ManageOrders()
        {
            var sellerId = GetCurrentUserId();
            ViewData["Title"] = "Quản lý Đơn hàng";

            var orders = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.OrderDetails.Any(od => od.Product.SellerId == sellerId))
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View(orders);
        }

        [HttpPost("orders/update-status/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int id, string status)
        {
            var sellerId = GetCurrentUserId();
            var order = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == id && 
                    o.OrderDetails.Any(od => od.Product.SellerId == sellerId));

            if (order != null)
            {
                order.Status = status;
                order.UpdatedAt = DateTime.Now;

                // Log activity
                await _activityLogService.LogActivityAsync(
                    GetCurrentUserId(),
                    "UPDATE_ORDER_STATUS",
                    $"Đã cập nhật trạng thái đơn hàng #{order.OrderId} thành: {status}",
                    HttpContext.Connection.RemoteIpAddress?.ToString()
                );

                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã cập nhật trạng thái đơn hàng thành công!";
            }

            return RedirectToAction(nameof(ManageOrders));
        }

        //--- Xem thống kê bán hàng ---
        [HttpGet("analytics")]
        public async Task<IActionResult> Analytics()
        {
            var sellerId = GetCurrentUserId();
            ViewData["Title"] = "Thống kê Bán hàng";

            // Thống kê theo tháng
            var monthlyStats = await GetMonthlySalesStats(sellerId);
            ViewData["MonthlyStats"] = monthlyStats;

            // Top sản phẩm bán chạy
            var topProducts = await _context.Products
                .Where(p => p.SellerId == sellerId)
                .Include(p => p.OrderDetails)
                .OrderByDescending(p => p.OrderDetails.Sum(od => od.Quantity))
                .Take(10)
                .Select(p => new
                {
                    p.ProductName,
                    TotalSold = p.OrderDetails.Sum(od => od.Quantity),
                    TotalRevenue = p.OrderDetails.Sum(od => od.Quantity * od.TotalPrice)
                })
                .ToListAsync();

            ViewData["TopProducts"] = topProducts;

            return View();
        }

        //--- Helper Methods ---
        private int GetCurrentUserId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return int.TryParse(userIdClaim, out int userId) ? userId : 0;
        }

        private static string? SanitizeUrlString(string? value)
        {
            if (string.IsNullOrWhiteSpace(value)) return null;
            var trimmed = value.Trim();
            if (trimmed.StartsWith("data:", StringComparison.OrdinalIgnoreCase)) return null;
            if (trimmed.Length > 255) return trimmed.Substring(0, 255);
            return trimmed;
        }

        private async Task<Dictionary<string, object>> GetMonthlySalesStats(int sellerId)
        {
            var endDate = DateTime.Now;
            var startDate = endDate.AddMonths(-6);

            var monthlyData = new Dictionary<string, object>();

            // Thống kê doanh thu theo tháng
            var rawMonthlyRevenue = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .Where(o => o.OrderDate.HasValue && 
                           o.OrderDate >= startDate &&
                           o.OrderDetails.Any(od => od.Product.SellerId == sellerId))
                .GroupBy(o => new { o.OrderDate.Value.Year, o.OrderDate.Value.Month })
                .Select(g => new
                {
                    Year = g.Key.Year,
                    Month = g.Key.Month,
                    Revenue = g.Sum(o => o.TotalAmount ?? 0),
                    OrderCount = g.Count()
                })
                .ToListAsync();

            var monthlyRevenue = rawMonthlyRevenue
                .Select(x => new
                {
                    Month = x.Year + "-" + x.Month.ToString("D2"),
                    x.Revenue,
                    x.OrderCount
                })
                .OrderBy(x => x.Month)
                .ToList();

            monthlyData["Revenue"] = monthlyRevenue;

            return monthlyData;
        }
    }
}
