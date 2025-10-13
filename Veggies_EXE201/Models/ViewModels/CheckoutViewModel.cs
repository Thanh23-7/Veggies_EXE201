using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using Veggies_EXE201.Models.ViewModels;

namespace Veggies_EXE201.ViewModels
{
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên người nhận.")]
        [Display(Name = "Họ và tên người nhận")]
        public string ShippingFullName { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ.")]
        [Display(Name = "Số điện thoại")]
        public string ShippingPhone { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng.")]
        [Display(Name = "Địa chỉ giao hàng")]
        public string ShippingAddress { get; set; }

        [Display(Name = "Ghi chú cho đơn hàng (tùy chọn)")]
        public string? OrderNotes { get; set; }

        // Danh sách các sản phẩm trong giỏ hàng để hiển thị lại ở trang checkout
        public List<CartItemViewModel> CartItems { get; set; } = new List<CartItemViewModel>();

        // Tổng số tiền của đơn hàng
        public decimal TotalAmount { get; set; }
    }

}