using System.Threading.Tasks;
using Veggies_EXE201.Models;

namespace Veggies_EXE201.Services
{
    public interface IPayOSService
    {
        /// <summary>
        /// Tạo link thanh toán PayOS cho một đơn hàng.
        /// </summary>
        /// <returns>Một chuỗi URL thanh toán hoặc null nếu có lỗi.</returns>
        Task<string?> CreatePaymentLink(Order order);
    }
}