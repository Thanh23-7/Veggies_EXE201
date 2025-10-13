using Net.payOS;
using Net.payOS.Types;
using Veggies_EXE201.Models;

namespace Veggies_EXE201.Services
{
    public class PayOSService : IPayOSService
    {
        private readonly PayOS _payOS;
        private readonly IConfiguration _configuration;

        public PayOSService(IConfiguration configuration)
        {
            _configuration = configuration;
            var clientId = _configuration["PayOS:ClientId"];
            var apiKey = _configuration["PayOS:ApiKey"];
            var checksumKey = _configuration["PayOS:ChecksumKey"];
            _payOS = new PayOS(clientId, apiKey, checksumKey);
        }

        public async Task<string?> CreatePaymentLink(Order order)
        {
            try
            {
                var successUrl = _configuration["PayOS:SuccessUrl"];
                var cancelUrl = _configuration["PayOS:CancelUrl"];

                // **1. Tạo danh sách các mặt hàng**
                // Mã nguồn mẫu cũng làm tương tự bước này.
                var items = new List<ItemData>();
                foreach (var detail in order.OrderDetails)
                {
                    items.Add(new ItemData(
                        detail.Product.ProductName, // Tên sản phẩm
                        detail.Quantity,            // Số lượng
                        (int)detail.UnitPrice       // Giá của 1 sản phẩm
                    ));
                }

                // **2. Tạo đối tượng PaymentData**
                // `order.OrderId` phải là một số nguyên duy nhất.
                var paymentData = new PaymentData(
                    order.OrderId,
                    (int)order.TotalAmount,
                    $"Thanh toan don hang #{order.OrderId}",
                    items,
                    cancelUrl,
                    successUrl
                );

                // **3. Tạo link thanh toán**
                CreatePaymentResult result = await _payOS.createPaymentLink(paymentData);
                return result.checkoutUrl;
            }
            catch (Exception ex)
            {
                // Ghi lại lỗi để dễ dàng debug
                Console.WriteLine($"Error creating PayOS link: {ex.Message}");
                return null;
            }
        }
    }
}