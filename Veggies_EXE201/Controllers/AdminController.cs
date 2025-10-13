//using Microsoft.AspNetCore.Authorization;
//using Microsoft.AspNetCore.Mvc;
//using System.Security.Claims;
//using System.Threading.Tasks;
//using Veggies_EXE201.Services; // Giả sử bạn đặt các interface service ở đây
//using Veggies_EXE201.Models;   // Giả sử bạn đặt các model ở đây

//namespace Veggies_EXE201.Controllers
//{
//    /// <summary>
//    /// Controller này quản lý tất cả các chức năng của trang Admin.
//    /// Yêu cầu người dùng phải đăng nhập và có vai trò "Admin".
//    /// </summary>
//    [Authorize(Roles = "Admin")]
//    [Route("admin")] // Route cơ sở gọn gàng hơn: /admin
//    public class AdminController : Controller
//    {
//        private readonly IUserService _userService;
//        private readonly IProductService _productService;
//        private readonly IReviewService _reviewService;
//        // private readonly IOrderService _orderService; // Ví dụ thêm quản lý đơn hàng

//        public AdminController(
//            IUserService userService,
//            IProductService productService,
//            IReviewService reviewService)
//        {
//            _userService = userService;
//            _productService = productService;
//            _reviewService = reviewService;
//        }

//        //--- Trang Dashboard chính của Admin ---
//        // GET: /admin hoặc /admin/index
//        [HttpGet]
//        [HttpGet("index")]
//        public IActionResult Index()
//        {
//            // Lấy tên của admin đang đăng nhập để hiển thị lời chào
//            string adminName = User.FindFirstValue(ClaimTypes.Name) ?? "Admin";
//            ViewData["AdminName"] = adminName;
//            return View(); // Trả về Views/Admin/Index.cshtml
//        }

//        //--- Quản lý Tài khoản (Users) ---
//        // GET: /admin/accounts
//        [HttpGet("accounts")]
//        public async Task<IActionResult> ManageAccounts()
//        {
//            ViewData["Title"] = "Quản lý Tài khoản";
//            var allUsers = await _userService.GetAllUsersAsync();
//            return View("Accounts/Index", allUsers); // Trả về Views/Admin/Accounts/Index.cshtml
//        }

//        // POST: /admin/accounts/ban/{id}
//        [HttpPost("accounts/ban/{id}")]
//        [ValidateAntiForgeryToken] // Bảo vệ chống tấn công CSRF
//        public async Task<IActionResult> BanUser(string id)
//        {
//            await _userService.BanUserAsync(id);
//            TempData["SuccessMessage"] = "Đã cấm người dùng thành công!";
//            return RedirectToAction(nameof(ManageAccounts));
//        }

//        // POST: /admin/accounts/unban/{id}
//        [HttpPost("accounts/unban/{id}")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> UnbanUser(string id)
//        {
//            await _userService.UnbanUserAsync(id);
//            TempData["SuccessMessage"] = "Đã bỏ cấm người dùng thành công!";
//            return RedirectToAction(nameof(ManageAccounts));
//        }

//        //--- Quản lý Sản phẩm (Products) ---
//        // GET: /admin/products
//        [HttpGet("products")]
//        public async Task<IActionResult> ManageProducts()
//        {
//            ViewData["Title"] = "Quản lý Sản phẩm";
//            var allProducts = await _productService.GetAllProductsAsync();
//            return View("Products/Index", allProducts); // Trả về Views/Admin/Products/Index.cshtml
//        }

//        // GET: /admin/products/create
//        [HttpGet("products/create")]
//        public IActionResult CreateProduct()
//        {
//            ViewData["Title"] = "Thêm Sản phẩm mới";
//            return View("Products/Create"); // Trả về form thêm mới
//        }

//        // POST: /admin/products/create
//        [HttpPost("products/create")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> CreateProduct(Product newProduct)
//        {
//            if (ModelState.IsValid)
//            {
//                await _productService.CreateProductAsync(newProduct);
//                TempData["SuccessMessage"] = "Đã thêm sản phẩm thành công!";
//                return RedirectToAction(nameof(ManageProducts));
//            }
//            // Nếu model không hợp lệ, quay lại form và báo lỗi
//            ViewData["Title"] = "Thêm Sản phẩm mới";
//            return View("Products/Create", newProduct);
//        }

//        // Tương tự, bạn sẽ tạo các Action cho Edit và Delete sản phẩm...

//        //--- Quản lý Đánh giá (Reviews) ---
//        // GET: /admin/reviews
//        [HttpGet("reviews")]
//        public async Task<IActionResult> ManageReviews()
//        {
//            ViewData["Title"] = "Quản lý Đánh giá";
//            var allReviews = await _reviewService.GetAllReviewsAsync();
//            return View("Reviews/Index", allReviews); // Trả về Views/Admin/Reviews/Index.cshtml
//        }

//        // POST: /admin/reviews/delete/{id}
//        [HttpPost("reviews/delete/{id}")]
//        [ValidateAntiForgeryToken]
//        public async Task<IActionResult> DeleteReview(int id)
//        {
//            await _reviewService.DeleteReviewAsync(id);
//            TempData["SuccessMessage"] = "Đã xóa đánh giá thành công!";
//            return RedirectToAction(nameof(ManageReviews));
//        }
//    }
//}