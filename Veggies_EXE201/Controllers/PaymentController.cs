using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veggies_EXE201.Models;
using Veggies_EXE201.ViewModels;
using Veggies_EXE201.Services;
using Veggies_EXE201.Helpers;
using Veggies_EXE201.Models.ViewModels;

public class PaymentController : Controller
{
    private readonly VeggiesDb2Context _context;
    private readonly IPayOSService _payOSService;

    public PaymentController(VeggiesDb2Context context, IPayOSService payOSService)
    {
        _context = context;
        _payOSService = payOSService;
    }

    // GET: /Payment/Checkout
    // Action này chịu trách nhiệm hiển thị trang checkout.
    [HttpGet]
    public IActionResult Checkout()
    {
        var cart = HttpContext.Session.Get<List<CartItemViewModel>>("MyCart") ?? new List<CartItemViewModel>();
        if (!cart.Any())
        {
            TempData["ErrorMessage"] = "Giỏ hàng của bạn đang trống!";
            return RedirectToAction("Index", "Cart");
        }

        var userId = HttpContext.Session.GetInt32("UserId");
        var user = _context.Users.Find(userId);

        var viewModel = new CheckoutViewModel
        {
            CartItems = cart,
            TotalAmount = cart.Sum(item => item.Total)
        };

        if (user != null)
        {
            viewModel.ShippingFullName = user.FullName;
            viewModel.ShippingPhone = user.Phone;
            viewModel.ShippingAddress = user.Address;
        }

        return View(viewModel);
    }

    // POST: /Payment/ProcessOrder
    // Action này xử lý khi người dùng nhấn nút "Đặt hàng".
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ProcessOrder(CheckoutViewModel model, string paymentMethod)
    {
        // 1. Lấy giỏ hàng từ Session bằng key chính xác
        var cart = HttpContext.Session.Get<List<CartItemViewModel>>("MyCart") ?? new List<CartItemViewModel>();

        // 2. Kiểm tra dữ liệu form có hợp lệ không
        if (!ModelState.IsValid)
        {
            // Nếu không hợp lệ, điền lại thông tin giỏ hàng và trả về form để người dùng sửa lỗi
            model.CartItems = cart;
            model.TotalAmount = cart.Sum(item => item.Total);
            return View("Checkout", model);
        }

        // 3. Kiểm tra lại giỏ hàng và trạng thái đăng nhập
        if (!cart.Any())
        {
            TempData["ErrorMessage"] = "Giỏ hàng trống, không thể đặt hàng.";
            return RedirectToAction("Index", "Cart");
        }

        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = "/Payment/Checkout" });
        }

        // 4. Tạo đối tượng Order và OrderDetail
        var order = new Order
        {
            UserId = userId.Value,
            OrderDate = DateTime.Now,
            Status = "Pending", // Trạng thái ban đầu
            TotalAmount = cart.Sum(item => item.Total),
            ShippingAddress = model.ShippingAddress,
            ShippingPhone = model.ShippingPhone,
            Notes = model.OrderNotes
        };

        foreach (var item in cart)
        {
            order.OrderDetails.Add(new OrderDetail
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.Price,
                TotalPrice = item.Total
            });
        }

        // 5. Lưu đơn hàng vào Database một cách an toàn
        try
        {
            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex) // Bắt lỗi cụ thể từ Database
        {
            // Ghi lại lỗi chi tiết để debug
            Console.WriteLine(ex.InnerException?.Message);
            TempData["ErrorMessage"] = "Lỗi khi lưu đơn hàng vào cơ sở dữ liệu. Vui lòng thử lại.";
            // Trả về trang checkout với dữ liệu đã điền
            model.CartItems = cart;
            model.TotalAmount = cart.Sum(item => item.Total);
            return View("Checkout", model);
        }

        // 6. Xử lý logic dựa trên phương thức thanh toán
        if (paymentMethod == "COD")
        {
            order.Status = "Pending"; // Đang xử lý
            await _context.SaveChangesAsync();
            HttpContext.Session.Remove("MyCart"); // Xóa giỏ hàng
            return RedirectToAction("OrderConfirmation", new { orderId = order.OrderId });
        }
        else if (paymentMethod == "PAYOS")
        {
            var orderForPayOS = await _context.Orders
                .Include(o => o.OrderDetails)
                .ThenInclude(od => od.Product)
                .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

            var paymentUrl = await _payOSService.CreatePaymentLink(orderForPayOS);

            if (!string.IsNullOrEmpty(paymentUrl))
            {
                return Redirect(paymentUrl); 
            }
            else
            {
                TempData["ErrorMessage"] = "Lỗi khi tạo link thanh toán. Vui lòng chọn phương thức khác.";
                return RedirectToAction("Checkout");
            }
        }
        return RedirectToAction("Checkout");
    }

    // GET: /Payment/PaymentSuccess
    [HttpGet]
    public async Task<IActionResult> PaymentSuccess([FromQuery] int orderCode)
    {
        var order = await _context.Orders.FindAsync(orderCode);
        if (order == null)
        {
            TempData["ErrorMessage"] = "Không tìm thấy đơn hàng được yêu cầu.";
            return RedirectToAction("Index", "Home");
        }

        if (order.Status == "Pending")
        {
            order.Status = "Paid"; 

            try
            {
                await _context.SaveChangesAsync(); 
                HttpContext.Session.Remove("MyCart"); 
            }
            catch (DbUpdateException ex)
            {
                Console.WriteLine($"Database update error in PaymentSuccess: {ex.InnerException?.Message}");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi cập nhật trạng thái đơn hàng của bạn.";
            }
        }

        return RedirectToAction("OrderConfirmation", new { orderId = orderCode });
    }

    // GET: /Payment/PaymentCancel
    [HttpGet]
    public async Task<IActionResult> PaymentCancel([FromQuery] int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order != null && order.Status == "Pending")
        {
            order.Status = "Cancelled";
            await _context.SaveChangesAsync();
        }
        TempData["ErrorMessage"] = "Thanh toán đã bị hủy.";
        return RedirectToAction("Checkout");
    }

    // GET: /Payment/OrderConfirmation/5
    public async Task<IActionResult> OrderConfirmation(int orderId)
    {
        var order = await _context.Orders.FindAsync(orderId);
        if (order == null) return NotFound();
        return View(order);
    }
}