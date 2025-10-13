using System;
using System.ComponentModel.DataAnnotations;

namespace Veggies_EXE201.ViewModels
{
    public class UserProfileViewModel
    {
        public string UserId { get; set; } // Hoặc int tùy vào hệ thống của bạn

        [Display(Name = "Họ và tên")]
        public string FullName { get; set; }

        [Display(Name = "Email")]
        public string Email { get; set; }

        [Display(Name = "Số điện thoại")]
        public string? PhoneNumber { get; set; }

        [Display(Name = "Địa chỉ")]
        public string? Address { get; set; }

        [Display(Name = "Ảnh đại diện")]
        public string? ProfilePictureUrl { get; set; }
    }
}