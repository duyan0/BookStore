using Microsoft.Extensions.Configuration;
using BookStore.Infrastructure.Services;
using BookStore.Core.DTOs;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Xunit;

namespace BookStore.Tests
{
    public class EmailNotificationTests
    {
        private readonly EmailService _emailService;

        public EmailNotificationTests()
        {
            // Create test configuration
            var configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(new Dictionary<string, string>
                {
                    {"Email:SmtpHost", "smtp.gmail.com"},
                    {"Email:SmtpPort", "587"},
                    {"Email:Username", "crandi21112004@gmail.com"},
                    {"Email:Password", "iljt phwd atnp syis"},
                    {"Email:FromEmail", "crandi21112004@gmail.com"},
                    {"Email:FromName", "BookStore - Test"}
                })
                .Build();

            _emailService = new EmailService(configuration);
        }

        [Fact]
        public async Task SendOrderConfirmationEmail_ShouldSendSuccessfully()
        {
            // Arrange
            var order = new OrderDto
            {
                Id = 123,
                OrderDate = DateTime.Now,
                TotalAmount = 250000,
                ShippingAddress = "123 Test Street, Ho Chi Minh City",
                PaymentMethod = "Cash",
                OrderDetails = new List<OrderDetailDto>
                {
                    new OrderDetailDto
                    {
                        BookTitle = "Test Book 1",
                        Quantity = 2,
                        UnitPrice = 100000,
                        TotalPrice = 200000
                    },
                    new OrderDetailDto
                    {
                        BookTitle = "Test Book 2", 
                        Quantity = 1,
                        UnitPrice = 50000,
                        TotalPrice = 50000
                    }
                }
            };

            // Act
            var result = await _emailService.SendOrderConfirmationEmailAsync(
                "crandi21112004@gmail.com", 
                order, 
                "Nguyễn Test User"
            );

            // Assert
            Assert.True(result, "Order confirmation email should be sent successfully");
        }

        [Fact]
        public async Task SendOrderStatusUpdateEmail_ShouldSendSuccessfully()
        {
            // Arrange
            var order = new OrderDto
            {
                Id = 124,
                OrderDate = DateTime.Now.AddDays(-1),
                TotalAmount = 150000,
                ShippingAddress = "456 Update Street, Ho Chi Minh City",
                PaymentMethod = "Credit Card"
            };

            // Act
            var result = await _emailService.SendOrderStatusUpdateEmailAsync(
                "crandi21112004@gmail.com",
                order,
                "Nguyễn Test User",
                "Pending",
                "Confirmed"
            );

            // Assert
            Assert.True(result, "Order status update email should be sent successfully");
        }

        [Fact]
        public async Task SendOrderCancellationEmail_ShouldSendSuccessfully()
        {
            // Arrange
            var order = new OrderDto
            {
                Id = 125,
                OrderDate = DateTime.Now.AddHours(-2),
                TotalAmount = 300000,
                ShippingAddress = "789 Cancel Street, Ho Chi Minh City",
                PaymentMethod = "Bank Transfer"
            };

            // Act
            var result = await _emailService.SendOrderCancellationEmailAsync(
                "crandi21112004@gmail.com",
                order,
                "Nguyễn Test User",
                "Khách hàng yêu cầu hủy đơn hàng"
            );

            // Assert
            Assert.True(result, "Order cancellation email should be sent successfully");
        }

        [Fact]
        public async Task SendOrderConfirmationEmail_WithInvalidEmail_ShouldReturnFalse()
        {
            // Arrange
            var order = new OrderDto
            {
                Id = 126,
                OrderDate = DateTime.Now,
                TotalAmount = 100000,
                ShippingAddress = "Test Address",
                PaymentMethod = "Cash",
                OrderDetails = new List<OrderDetailDto>()
            };

            // Act
            var result = await _emailService.SendOrderConfirmationEmailAsync(
                "", // Invalid email
                order,
                "Test User"
            );

            // Assert
            Assert.False(result, "Should return false for invalid email");
        }
    }
}
