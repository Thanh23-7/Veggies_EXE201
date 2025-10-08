using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
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
        public IActionResult Login()
        {
            return View(); // Trả về View chứa form đăng nhập
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken] // Nên có để chống tấn công CSRF
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            // Kiểm tra Validation của ViewModel
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. Xác thực người dùng bằng Service
            var user = await _authService.ValidateUserCredentials(model);

            if (user == null)
            {
                // Thêm thông báo lỗi nếu xác thực thất bại
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không đúng.");
                return View(model);
            }

            // 2. Tạo Claims Identity cho Cookie Authentication
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.UserId.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Name, user.FullName),
                new Claim(ClaimTypes.Role, user.Role ?? "User") // Sử dụng trường Role
            };

            var claimsIdentity = new ClaimsIdentity(
                claims, "CookieAuth");

            var authProperties = new AuthenticationProperties
            {
                // Đặt thời gian duy trì đăng nhập dựa trên RememberMe
                IsPersistent = model.RememberMe,
                ExpiresUtc = model.RememberMe ? DateTimeOffset.UtcNow.AddDays(7) : DateTimeOffset.UtcNow.AddMinutes(30)
            };

            // 3. Đăng nhập người dùng (tạo Cookie)
            await HttpContext.SignInAsync(
                "CookieAuth", // Tên Scheme đã đăng ký trong Program.cs
                new ClaimsPrincipal(claimsIdentity),
                authProperties);

            // 4. Chuyển hướng người dùng
            if (Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home"); // Chuyển hướng về trang chủ
        }
        // GET: /Account/Register
        [HttpGet]
        public IActionResult Register()
        {
            var model = new RegisterViewModel();
            return View(model); // Trả về View chứa form đăng ký
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Gọi service để đăng ký người dùng
            var (success, error) = await _authService.RegisterUser(model);

            if (success)
            {
                // Đăng ký thành công, chuyển hướng đến trang đăng nhập
                TempData["SuccessMessage"] = "Đăng ký thành công! Vui lòng đăng nhập.";
                return RedirectToAction("Login");
            }
            else
            {
                // Đăng ký thất bại (ví dụ: email đã tồn tại)
                ModelState.AddModelError(string.Empty, error);
                return View(model);
            }
        }

        // POST: /Account/Logout
        [HttpPost]
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync("CookieAuth");
            return RedirectToAction("Index", "Home");
        }
    }
}