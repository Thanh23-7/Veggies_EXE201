using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using Microsoft.AspNetCore.Mvc.Rendering;
using System;

namespace Veggies_EXE201.Models.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Tên đầy đủ là bắt buộc.")]
        [Display(Name = "Tên")]
        public string FullName { get; set; } = null!;

        [Required(ErrorMessage = "Email là bắt buộc.")]
        [EmailAddress(ErrorMessage = "Địa chỉ Email không hợp lệ.")]
        [Display(Name = "Email")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "Mật khẩu là bắt buộc.")]
        [StringLength(100, ErrorMessage = "{0} phải có ít nhất {2} và tối đa {1} ký tự.", MinimumLength = 6)]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = null!;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu và xác nhận mật khẩu không khớp.")]
        public string ConfirmPassword { get; set; } = null!;

        // THÔNG TIN BỔ SUNG

        [Display(Name = "Số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        public string? Phone { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "URL Ảnh đại diện")]
        [Url(ErrorMessage = "URL ảnh không hợp lệ.")]
        public string? AvatarUrl { get; set; }

        // CÁC TRƯỜNG MỚI ĐƯỢC THÊM VÀO
        [Display(Name = "Ngày sinh")]
        [DataType(DataType.Date)]
        public DateOnly? DateOfBirth { get; set; }

        [Display(Name = "Giới tính")]
        public string? Gender { get; set; }


        // THUỘC TÍNH MỚI: Role được chọn bởi người dùng

        [Required(ErrorMessage = "Vui lòng chọn vai trò.")]

        [Display(Name = "Vai trò")]

        public string SelectedRole { get; set; } = "Customer"; // Thiết lập Customer làm mặc định



        // THUỘC TÍNH MỚI: Dùng để điền dữ liệu cho thẻ <select> trong View

        public List<SelectListItem> AvailableRoles { get; set; } = new List<SelectListItem>

        {

            new SelectListItem { Value = "Customer", Text = "Customer" },

            new SelectListItem { Value = "Seller", Text = "Seller" }

            // Lưu ý: Không cho phép chọn Admin khi đăng ký

        };
    }
}
