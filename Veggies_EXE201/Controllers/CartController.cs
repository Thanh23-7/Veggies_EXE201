using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Veggies_EXE201.Helpers;
using Veggies_EXE201.Models.ViewModels;
using Veggies_EXE201.Services;

namespace Veggies_EXE201.Controllers
{
    public class CartController : Controller
    {
        private readonly IProductService _productService;
        public const string CARTKEY = "cart"; // Key để lưu giỏ hàng trong Session

        public CartController(IProductService productService)
        {
            _productService = productService;
        }

        // Lấy giỏ hàng từ Session
        private CartViewModel GetCart()
        {
            return HttpContext.Session.Get<CartViewModel>(CARTKEY) ?? new CartViewModel();
        }

        // Lưu giỏ hàng vào Session
        private void SaveCart(CartViewModel cart)
        {
            HttpContext.Session.Set(CARTKEY, cart);
        }

        // Action để hiển thị trang giỏ hàng
        public IActionResult Index()
        {
            return View(GetCart());
        }

        // Action để thêm sản phẩm vào giỏ
        [HttpPost]
        public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
        {
            var cart = GetCart();
            var cartItem = cart.CartItems.SingleOrDefault(p => p.ProductId == productId);

            if (cartItem == null) // Nếu sản phẩm chưa có trong giỏ
            {
                var product = await _productService.GetProductByIdAsync(productId);
                if (product == null)
                {
                    return NotFound();
                }
                cartItem = new CartItemViewModel
                {
                    ProductId = product.ProductId,
                    ProductName = product.ProductName,
                    ImageUrl = product.ProductImage ?? "/images/default-product.png",
                    Price = product.Price,
                    Quantity = quantity
                };
                cart.CartItems.Add(cartItem);
            }
            else // Nếu sản phẩm đã có, tăng số lượng
            {
                cartItem.Quantity += quantity;
            }

            SaveCart(cart);
            return RedirectToAction("Index"); // Chuyển hướng đến trang giỏ hàng
        }

        // Các action khác như Remove, Update... (sẽ làm sau nếu cần)
    }
}