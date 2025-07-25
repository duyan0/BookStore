using BookStore.Core.DTOs;
using BookStore.Core.Services;
using Microsoft.Extensions.DependencyInjection;

namespace BookStore.API
{
    public static class TestEmailNotifications
    {
        public static async Task RunEmailTests(IServiceProvider serviceProvider)
        {
            var emailService = serviceProvider.GetRequiredService<IEmailService>();
            
            Console.WriteLine("🧪 === TESTING EMAIL NOTIFICATIONS ===");
            
            // Test 1: Order Confirmation Email
            Console.WriteLine("\n📧 Test 1: Order Confirmation Email");
            var testOrder = new OrderDto
            {
                Id = 999,
                OrderDate = DateTime.Now,
                TotalAmount = 250000,
                ShippingAddress = "123 Test Street, Ho Chi Minh City",
                PaymentMethod = "Cash",
                OrderDetails = new List<OrderDetailDto>
                {
                    new OrderDetailDto
                    {
                        BookTitle = "Lập trình C# cơ bản",
                        Quantity = 2,
                        UnitPrice = 100000
                    },
                    new OrderDetailDto
                    {
                        BookTitle = "ASP.NET Core MVC",
                        Quantity = 1,
                        UnitPrice = 50000
                    }
                }
            };

            var result1 = await emailService.SendOrderConfirmationEmailAsync(
                "crandi21112004@gmail.com",
                testOrder,
                "Nguyễn Test User"
            );
            Console.WriteLine($"✅ Order Confirmation Email: {(result1 ? "SUCCESS" : "FAILED")}");

            // Test 2: Order Status Update Email
            Console.WriteLine("\n📧 Test 2: Order Status Update Email");
            var result2 = await emailService.SendOrderStatusUpdateEmailAsync(
                "crandi21112004@gmail.com",
                testOrder,
                "Nguyễn Test User",
                "Pending",
                "Confirmed"
            );
            Console.WriteLine($"✅ Order Status Update Email: {(result2 ? "SUCCESS" : "FAILED")}");

            // Test 3: Order Cancellation Email
            Console.WriteLine("\n📧 Test 3: Order Cancellation Email");
            var result3 = await emailService.SendOrderCancellationEmailAsync(
                "crandi21112004@gmail.com",
                testOrder,
                "Nguyễn Test User",
                "Khách hàng yêu cầu hủy đơn hàng do thay đổi ý định"
            );
            Console.WriteLine($"✅ Order Cancellation Email: {(result3 ? "SUCCESS" : "FAILED")}");

            Console.WriteLine("\n🎯 === EMAIL NOTIFICATION TESTS COMPLETED ===");
            Console.WriteLine($"📊 Results: {(result1 ? 1 : 0) + (result2 ? 1 : 0) + (result3 ? 1 : 0)}/3 tests passed");
        }
    }
}
