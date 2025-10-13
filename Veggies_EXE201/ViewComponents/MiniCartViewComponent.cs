using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Veggies_EXE201.Models.ViewModels;
using Veggies_EXE201.Models;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Veggies_EXE201.ViewComponents
{
    public class MiniCartViewComponent : ViewComponent
    {
        private readonly VeggiesDb2Context _context;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public MiniCartViewComponent(VeggiesDb2Context context, IHttpContextAccessor httpContextAccessor)
        {
            _context = context;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<IViewComponentResult> InvokeAsync()
        {
            var claimsPrincipal = _httpContextAccessor.HttpContext?.User;

            // Lấy UserId dưới dạng chuỗi từ claims
            var userIdString = claimsPrincipal?.FindFirstValue(ClaimTypes.NameIdentifier);

            var cartItemsViewModel = new List<CartItemViewModel>();

            // Chỉ thực hiện truy vấn nếu người dùng đã đăng nhập và UserId hợp lệ
            // << FIX 1: Chuyển đổi UserId từ string sang int một cách an toàn
            if (!string.IsNullOrEmpty(userIdString) && int.TryParse(userIdString, out int userId))
            {
                cartItemsViewModel = await _context.CartDetails
                    .Include(cd => cd.Product)
                    // Sử dụng userId (kiểu int) đã được chuyển đổi để lọc
                    .Where(cd => cd.ShoppingCart.UserId == userId)
                    .Select(cd => new CartItemViewModel
                    {
                        ProductId = cd.Product.ProductId, // << Đảm bảo tên thuộc tính khớp model của bạn
                        ProductName = cd.Product.ProductName,
                        Price = cd.Product.Price,
                        ProductImage = cd.Product.ProductImage, // << Đảm bảo tên thuộc tính khớp model của bạn

                        // << FIX 2: Thêm số lượng sản phẩm từ CartDetail
                        Quantity = cd.Count
                    })
                    .ToListAsync();
            }

            // Trả về view với dữ liệu thật hoặc một danh sách rỗng
            return View(cartItemsViewModel);
        }
    }
}