using Microsoft.AspNetCore.Mvc;
using Veggies_EXE201.Services;
using Microsoft.AspNetCore.Http; // Giữ lại nếu bạn cần dùng IFormCollection cho các Action CRUD sau này

namespace Veggies_EXE201.Controllers
{
    // Kế thừa từ Controller
    public class ProductController : Controller
    {
        // Khai báo Service để sử dụng logic nghiệp vụ
        private readonly IProductService _productService;

        // Constructor: Dependency Injection để tiêm IProductService
        public ProductController(IProductService productService)
        {
            _productService = productService;
        }

        /// <summary>
        /// Action mặc định để hiển thị trang danh sách sản phẩm với các bộ lọc.
        /// Nhận các tham số từ Query String để thực hiện tìm kiếm, lọc và phân trang.
        /// </summary>
        /// <param name="categoryId">ID danh mục được chọn.</param>
        /// <param name="searchTerm">Từ khóa tìm kiếm.</param>
        /// <param name="sortOrder">Thứ tự sắp xếp (priceAsc, priceDesc...).</param>
        /// <param name="minPrice">Mức giá thấp nhất để lọc.</param>
        /// <param name="maxPrice">Mức giá cao nhất để lọc.</param>
        /// <param name="page">Số trang hiện tại (mặc định là 1).</param>
        /// <returns>View chứa ProductListViewModel đã được xử lý.</returns>
        public async Task<IActionResult> Index(
            int? categoryId,
            string searchTerm,
            string sortOrder,
            decimal? minPrice,
            decimal? maxPrice,
            int page = 1)
        {
            var viewModel = await _productService.GetProductsViewModelAsync(
       searchTerm,
       categoryId,
       minPrice,
       maxPrice,
       sortOrder,
       page);

            // Return the View with the ViewModel
            return View(viewModel);
        }

        // Action Xem chi tiết sản phẩm
        // === THÊM ACTION MỚI CHO TRANG CHI TIẾT ===
        // GET: /Product/Details/5
        public async Task<IActionResult> Details(int id)
        {
            // Gọi service để lấy sản phẩm theo ID
            var product = await _productService.GetProductByIdAsync(id);

            // Nếu không tìm thấy sản phẩm, trả về trang 404 Not Found
            if (product == null)
            {
                return NotFound();
            }

            // Truyền đối tượng sản phẩm cho View
            return View(product);
        }

        /* * Các Action CRUD (Create, Edit, Delete) đã bị loại bỏ/comment để tập trung vào chức năng Shop, 
         * nhưng bạn có thể thêm lại khi cần triển khai logic quản trị sản phẩm.
         */

        // // GET: ProductController/Create
        // public ActionResult Create()
        // {
        //     return View();
        // }

        // // POST: ProductController/Create
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public ActionResult Create(IFormCollection collection)
        // {
        //     try { return RedirectToAction(nameof(Index)); }
        //     catch { return View(); }
        // }

        // // GET: ProductController/Edit/5
        // public ActionResult Edit(int id)
        // {
        //     return View();
        // }

        // // POST: ProductController/Edit/5
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public ActionResult Edit(int id, IFormCollection collection)
        // {
        //     try { return RedirectToAction(nameof(Index)); }
        //     catch { return View(); }
        // }

        // // GET: ProductController/Delete/5
        // public ActionResult Delete(int id)
        // {
        //     return View();
        // }

        // // POST: ProductController/Delete/5
        // [HttpPost]
        // [ValidateAntiForgeryToken]
        // public ActionResult Delete(int id, IFormCollection collection)
        // {
        //     try { return RedirectToAction(nameof(Index)); }
        //     catch { return View(); }
        // }
    }
}
