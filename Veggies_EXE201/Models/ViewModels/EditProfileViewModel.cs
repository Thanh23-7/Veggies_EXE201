using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http; // Required for IFormFile

namespace Veggies_EXE201.ViewModels
{
    public class EditProfileViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên.")]
        [StringLength(100)]
        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        // Dùng để tải ảnh mới lên
        [Display(Name = "Ảnh đại diện mới")]
        public IFormFile? NewProfilePicture { get; set; }

        // Dùng để hiển thị ảnh hiện tại
        public string? ExistingProfilePictureUrl { get; set; }
    }
}