using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Veggies_EXE201.Models;
using Veggies_EXE201.ViewModels;
using System.Threading.Tasks;
using System.IO;

public class UserController : Controller
{
    private readonly VeggiesDb2Context _context;
    private readonly IWebHostEnvironment _webHostEnvironment;

    // Chỉ cần inject DbContext và IWebHostEnvironment
    public UserController(VeggiesDb2Context context, IWebHostEnvironment webHostEnvironment)
    {
        _context = context;
        _webHostEnvironment = webHostEnvironment;
    }

    // --- READ (Xem hồ sơ) ---
    // GET: /User/Profile
    public async Task<IActionResult> Profile()
    {
        // Giả sử bạn lưu UserId trong Session sau khi đăng nhập
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Account"); // Chuyển đến trang đăng nhập nếu chưa có session
        }

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
        {
            return NotFound();
        }

        return View(user); // Truyền thẳng model User vào View
    }

    // --- UPDATE (Chỉnh sửa hồ sơ) ---
    // GET: /User/EditProfile
    public async Task<IActionResult> EditProfile()
    {
        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return RedirectToAction("Login", "Account");
        }

        var user = await _context.Users.FindAsync(userId.Value);
        if (user == null)
        {
            return NotFound();
        }

        // Tạo ViewModel từ dữ liệu người dùng để điền vào form
        var viewModel = new EditProfileViewModel
        {
            FullName = user.FullName,
            PhoneNumber = user.Phone,
            Address = user.Address,
            ExistingProfilePictureUrl = user.AvatarUrl
        };

        return View(viewModel);
    }

    // POST: /User/EditProfile
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> EditProfile(EditProfileViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model); // Trả về form nếu dữ liệu không hợp lệ
        }

        var userId = HttpContext.Session.GetInt32("UserId");
        if (userId == null)
        {
            return Unauthorized();
        }

        var userToUpdate = await _context.Users.FindAsync(userId.Value);
        if (userToUpdate == null)
        {
            return NotFound();
        }

        // 1. Cập nhật thông tin từ form vào model
        userToUpdate.FullName = model.FullName;
        userToUpdate.Phone = model.PhoneNumber;
        userToUpdate.Address = model.Address;

        // 2. Xử lý upload ảnh mới
        if (model.NewProfilePicture != null && model.NewProfilePicture.Length > 0)
        {
            // Xóa ảnh cũ
            if (!string.IsNullOrEmpty(userToUpdate.AvatarUrl))
            {
                string oldImagePath = Path.Combine(_webHostEnvironment.WebRootPath, userToUpdate.AvatarUrl.TrimStart('/'));
                if (System.IO.File.Exists(oldImagePath))
                {
                    System.IO.File.Delete(oldImagePath);
                }
            }

            // Lưu ảnh mới
            string uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images", "avatars");
            Directory.CreateDirectory(uploadsFolder);
            string uniqueFileName = Guid.NewGuid().ToString() + "_" + model.NewProfilePicture.FileName;
            string newImagePath = Path.Combine(uploadsFolder, uniqueFileName);
            using (var fileStream = new FileStream(newImagePath, FileMode.Create))
            {
                await model.NewProfilePicture.CopyToAsync(fileStream);
            }
            userToUpdate.AvatarUrl = $"/images/avatars/{uniqueFileName}";
        }

        // 3. Lưu vào CSDL
        try
        {
            await _context.SaveChangesAsync();
            TempData["SuccessMessage"] = "Cập nhật hồ sơ thành công!";
            return RedirectToAction("Profile");
        }
        catch (DbUpdateException)
        {
            ModelState.AddModelError("", "Không thể lưu thay đổi. Vui lòng thử lại.");
            return View(model);
        }
    }
}