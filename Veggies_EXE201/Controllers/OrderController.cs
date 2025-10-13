using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veggies_EXE201.Models;
using Veggies_EXE201.ViewModels;
using System.Linq;
using System.Threading.Tasks;

public class OrderController : Controller
{
    private readonly VeggiesDb2Context _context;

    public OrderController(VeggiesDb2Context context)
    {
        _context = context;
    }

    // GET: /Order/History
    public async Task<IActionResult> History()
    {
        // 1. Lấy ID của người dùng đang đăng nhập từ Session
        var userId = HttpContext.Session.GetInt32("UserId");

        // 2. Nếu người dùng chưa đăng nhập, chuyển hướng đến trang đăng nhập
        if (userId == null)
        {
            return RedirectToAction("Login", "Account", new { returnUrl = "/Order/History" });
        }

        // 3. Truy vấn các đơn hàng của người dùng đó từ CSDL
        var userOrders = await _context.Orders
            .Where(o => o.UserId == userId.Value)
            .Include(o => o.OrderDetails) // Lấy cả chi tiết đơn hàng để đếm sản phẩm
            .OrderByDescending(o => o.OrderDate) // Sắp xếp đơn hàng mới nhất lên đầu
            .Select(o => new OrderSummaryViewModel // Chuyển đổi sang ViewModel
            {
                OrderId = o.OrderId,
                OrderDate = o.OrderDate,
                TotalAmount = o.TotalAmount,
                Status = o.Status,
                ItemCount = o.OrderDetails.Count()
            })
            .ToListAsync();

        var viewModel = new OrderHistoryViewModel
        {
            Orders = userOrders
        };

        return View(viewModel);
    }

    // GET: /Order/Details/5
    // Action này để xem chi tiết một đơn hàng cụ thể
    public async Task<IActionResult> Details(int id)
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var order = await _context.Orders
            .Include(o => o.OrderDetails)
            .ThenInclude(od => od.Product)
            .FirstOrDefaultAsync(o => o.OrderId == id && o.UserId == userId.Value);

        if (order == null)
        {
            return NotFound();
        }

        return View(order);
    }
}