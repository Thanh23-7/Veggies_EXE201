// /ViewComponents/ShoppingCartViewComponent.cs

using Microsoft.AspNetCore.Mvc;
// Thêm các using cần thiết để lấy dữ liệu giỏ hàng, ví dụ từ Session.

public class ShoppingCartViewComponent : ViewComponent
{
    public IViewComponentResult Invoke()
    {
        // Logic để lấy số lượng sản phẩm trong giỏ hàng
        // Ví dụ: lấy từ Session
        int cartItemCount = HttpContext.Session.GetInt32("CartItemCount") ?? 0;

        return View(cartItemCount); // Truyền số lượng vào view
    }
}