using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Veggies_EXE201.Models;
using Veggies_EXE201.Models.ViewModels;
using Veggies_EXE201.Services;

namespace Veggies_EXE201.Controllers
{
    public class HomeController : Controller
    {
        private readonly IProductService _productService;
        private readonly ILogger<HomeController> _logger;

        public HomeController(ILogger<HomeController> logger, IProductService productService)
        {
            _logger = logger;
            _productService = productService;
        }

        public async Task<IActionResult> Index()
        {
            // Lấy dữ liệu sản phẩm từ service
            var productData = await _productService.GetProductsViewModelAsync(
                searchTerm: null,
                categoryId: null,
                minPrice: null,
                maxPrice: null,
                sortBy: "name_asc", // Sắp xếp bất kỳ để lấy dữ liệu
                pageNumber: 1
            );

            // Lấy 4 sản phẩm đầu tiên để làm gợi ý
            var suggestedProducts = productData.Products.Take(4).ToList();

            // Truyền danh sách sản phẩm gợi ý cho View
            return View(suggestedProducts);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}