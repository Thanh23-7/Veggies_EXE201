using System.ComponentModel.DataAnnotations;

namespace Veggies_EXE201.Models.ViewModels
{
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Email không hợp lệ.")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;

        [Display(Name = "Ghi nhớ tôi?")]
        public bool RememberMe { get; set; }
    }
}