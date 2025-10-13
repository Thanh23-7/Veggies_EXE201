using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veggies_EXE201.Models;
using Veggies_EXE201.ViewModels;
using Veggies_EXE201.Helpers;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Veggies_EXE201.Models.ViewModels;

public class CartController : Controller
{
    private readonly VeggiesDb2Context _context;
    private const string CartSessionKey = "MyCart"; 

    public CartController(VeggiesDb2Context context)
    {
        _context = context;
    }

    // GET: /Cart/Index - Hiển thị giỏ hàng
    public IActionResult Index()
    {
        var cartItems = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionKey) ?? new List<CartItemViewModel>();
        var viewModel = new CartViewModel { CartItems = cartItems };
        return View(viewModel);
    }

    // POST: /Cart/AddToCart - Thêm sản phẩm
    [HttpPost]
    public async Task<IActionResult> AddToCart(int productId, int quantity = 1)
    {
        var product = await _context.Products.FindAsync(productId);
        if (product == null) return NotFound();

        var cart = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionKey) ?? new List<CartItemViewModel>();
        var existingItem = cart.FirstOrDefault(item => item.ProductId == productId);

        if (existingItem != null)
        {
            existingItem.Quantity += quantity;
        }
        else
        {
            cart.Add(new CartItemViewModel
            {
                ProductId = product.ProductId,
                ProductName = product.ProductName,
                ProductImage = product.ProductImage,
                Price = product.Price,
                Quantity = quantity
            });
        }

        HttpContext.Session.Set(CartSessionKey, cart);
        return RedirectToAction("Index");
    }

    // === PHẦN BỊ THIẾU BẮT ĐẦU TỪ ĐÂY ===

    // POST: /Cart/UpdateCart - Cập nhật số lượng
    [HttpPost]
    public IActionResult UpdateCart(int productId, int quantity)
    {
        var cart = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionKey);
        if (cart != null)
        {
            var itemToUpdate = cart.FirstOrDefault(item => item.ProductId == productId);
            if (itemToUpdate != null)
            {
                if (quantity > 0)
                {
                    itemToUpdate.Quantity = quantity; // Cập nhật số lượng mới
                }
                else
                {
                    cart.Remove(itemToUpdate); // Nếu số lượng <= 0, xóa sản phẩm
                }
                HttpContext.Session.Set(CartSessionKey, cart);
            }
        }
        return RedirectToAction("Index");
    }

    // POST: /Cart/RemoveFromCart - Xóa một sản phẩm
    [HttpPost]
    public IActionResult RemoveFromCart(int productId)
    {
        var cart = HttpContext.Session.Get<List<CartItemViewModel>>(CartSessionKey);
        if (cart != null)
        {
            var itemToRemove = cart.FirstOrDefault(item => item.ProductId == productId);
            if (itemToRemove != null)
            {
                cart.Remove(itemToRemove);
                HttpContext.Session.Set(CartSessionKey, cart);
            }
        }
        return RedirectToAction("Index");
    }
}