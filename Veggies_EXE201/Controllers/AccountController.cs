using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Security.Claims;
using Veggies_EXE201.Models; 
using Veggies_EXE201.Models.ViewModels;
using Veggies_EXE201.Services;

namespace Veggies_EXE201.Controllers
{
    public class AccountController : Controller
    {
        private readonly AuthService _authService;

        public AccountController(AuthService authService)
        {
            _authService = authService;
        }

        // GET: /Account/Login
        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Xác thực người dùng
            var user = await _authService.ValidateUserCredentials(model);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            // 2. Tạo Claims (các "thẻ bài" thông tin về người dùng)
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                // Lấy Role từ CSDL, nếu không có thì mặc định là "Customer"
                new Claim(ClaimTypes.Role, user.Role ?? "Customer")
            };

            var claimsIdentity = new ClaimsIdentity(claims, "CookieAuth");
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = model.RememberMe, // Lưu cookie lâu dài nếu người dùng chọn "Remember Me"
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(14) : null
            };

            // 3. Đăng nhập người dùng (tạo cookie xác thực)
            await HttpContext.SignInAsync("CookieAuth", new ClaimsPrincipal(claimsIdentity), authProperties);

            // 4. (Tùy chọn) Lưu UserId vào Session để truy cập nhanh
            HttpContext.Session.SetInt32("UserId", user.UserId);

            // 5. Chuyển hướng an toàn dựa trên role
            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }
            else
            {
                // Chuyển hướng dựa trên role
                if (user.Role == "Admin")
                {
                    return RedirectToAction("Index", "Admin");
                }
                else if (user.Role == "Seller")
                {
                    return RedirectToAction("Index", "Seller");
                }
                return RedirectToAction("Index", "Home");
            }
        }

        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            ViewBag.AvailableRoles = GetRoles();
            return View();
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.AvailableRoles = GetRoles(); // Lỗi: phải tải lại danh sách Role
                return View(model);
            }

            var (success, error) = await _authService.RegisterUser(model);

            if (success)
            {
                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            else
            {
                ModelState.AddModelError(string.Empty, error);
                ViewBag.AvailableRoles = GetRoles(); // Lỗi: phải tải lại danh sách Role
                return View(model);
            }
        }

        // POST: /Account/Logout
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            // Xóa cookie xác thực
            await HttpContext.SignOutAsync("CookieAuth");
            // Xóa toàn bộ dữ liệu Session
            HttpContext.Session.Clear();

            return RedirectToAction("Index", "Home");
        }

        // Hàm helper để tránh lặp code
        private List<SelectListItem> GetRoles()
        {
            return new List<SelectListItem>
            {
                new SelectListItem { Value = "Customer", Text = "Khách hàng" },
                new SelectListItem { Value = "Seller", Text = "Người bán" }
                // Không hiển thị "Admin" công khai để bảo mật
            };
        }

        // GET: /Account/AccessDenied
        [HttpGet]
        public IActionResult AccessDenied()
        {
            return View();
        }
    }
}