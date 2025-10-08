using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore; 
using Veggies_EXE201.Models;
using Veggies_EXE201.Models.ViewModels;

namespace Veggies_EXE201.Services
{
    public class AuthService
    {
        private readonly VeggiesDb2Context _context;
        private readonly IPasswordHasher<User> _passwordHasher;

        public AuthService(VeggiesDb2Context context, IPasswordHasher<User> passwordHasher)
        {
            _context = context;
            _passwordHasher = passwordHasher;
        }

        public async Task<User?> ValidateUserCredentials(LoginViewModel model)
        {
            // 1. Tìm người dùng theo Email
            var user = await _context.Users
                                     .FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                return null; // Không tìm thấy người dùng
            }

            // 2. Xác minh mật khẩu
            // `_passwordHasher` sẽ tự động xác minh mật khẩu thô (model.Password) 
            // với mật khẩu đã băm (user.PasswordHash)
            var result = _passwordHasher.VerifyHashedPassword(user, user.PasswordHash, model.Password);

            if (result == PasswordVerificationResult.Failed)
            {
                return null; // Mật khẩu không đúng
            }

            // 3. Xác thực thành công
            return user;
        }
        // Phương thức ĐĂNG KÝ
        public async Task<(bool success, string error)> RegisterUser(RegisterViewModel model)
        {
            // 1. Kiểm tra Email đã tồn tại chưa
            if (await _context.Users.AnyAsync(u => u.Email == model.Email))
            {
                return (false, "Email này đã được sử dụng.");
            }

            // 2. Tạo đối tượng User mới
            var newUser = new User
            {
                FullName = model.FullName,
                Email = model.Email,

                // CẬP NHẬT: Gán Role bằng giá trị người dùng đã chọn
                // Giá trị này phải khớp với ràng buộc CHECK trong DB ('Customer' hoặc 'Seller')
                Role = model.SelectedRole,

                CreatedAt = DateTime.UtcNow
            };

            // 3. BĂM MẬT KHẨU
            var hashedPassword = _passwordHasher.HashPassword(newUser, model.Password);
            newUser.PasswordHash = hashedPassword;

            // 4. Lưu vào cơ sở dữ liệu
            try
            {
                _context.Users.Add(newUser);
                await _context.SaveChangesAsync();
                return (true, string.Empty);
            }
            catch (DbUpdateException ex)
            {
                // ... (Logic bắt lỗi như đã hướng dẫn) ...
                // Kiểm tra lỗi độ dài (PasswordHash) và lỗi Role CHECK constraint
                Console.WriteLine($"InnerException: {ex.InnerException?.Message}");

                // Ví dụ: Bắt lỗi Role CHECK Constraint nếu giá trị không hợp lệ
                if (ex.InnerException?.Message.Contains("CHECK constraint") == true)
                {
                    return (false, "Lỗi cơ sở dữ liệu: Giá trị vai trò không hợp lệ.");
                }

                return (false, "Lỗi cơ sở dữ liệu: Không thể lưu dữ liệu do vi phạm ràng buộc.");
            }
            catch (Exception ex)
            {
                return (false, "Lỗi không xác định khi lưu dữ liệu.");
            }
        }
    }
}