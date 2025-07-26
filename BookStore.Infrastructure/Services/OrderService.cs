using AutoMapper;
using BookStore.Core.DTOs;
using BookStore.Core.Entities;
using BookStore.Core.Interfaces;
using BookStore.Core.Services;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;
        private readonly IEmailService _emailService;
        private readonly IUserRepository _userRepository;
        private readonly ApplicationDbContext _context;

        public OrderService(IOrderRepository orderRepository, IBookRepository bookRepository, IMapper mapper, IEmailService emailService, IUserRepository userRepository, ApplicationDbContext context)
        {
            _orderRepository = orderRepository;
            _bookRepository = bookRepository;
            _mapper = mapper;
            _emailService = emailService;
            _userRepository = userRepository;
            _context = context;
        }

        public async Task<IEnumerable<OrderDto>> GetAllOrdersAsync()
        {
            var orders = await _orderRepository.GetAllOrdersWithDetailsAsync();
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId)
        {
            var orders = await _orderRepository.GetOrdersByUserIdAsync(userId);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<OrderDto?> GetOrderByIdAsync(int id)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(id);
            return order == null ? null : _mapper.Map<OrderDto>(order);
        }

        public async Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto)
        {
            var order = new Order
            {
                UserId = createOrderDto.UserId,
                OrderDate = DateTime.UtcNow,
                Status = "Pending",
                ShippingAddress = createOrderDto.ShippingAddress,
                PaymentMethod = createOrderDto.PaymentMethod,
                OrderDetails = new List<OrderDetail>(),

                // Voucher fields
                VoucherCode = createOrderDto.VoucherCode,
                VoucherDiscount = createOrderDto.VoucherDiscount,
                FreeShipping = createOrderDto.FreeShipping,
                ShippingFee = createOrderDto.ShippingFee,
                SubTotal = createOrderDto.SubTotal
            };

            decimal totalAmount = 0;

            foreach (var detailDto in createOrderDto.OrderDetails)
            {
                var book = await _bookRepository.GetByIdAsync(detailDto.BookId);
                if (book == null || book.Quantity < detailDto.Quantity)
                {
                    throw new InvalidOperationException($"Insufficient stock for book ID {detailDto.BookId}");
                }

                var orderDetail = new OrderDetail
                {
                    BookId = detailDto.BookId,
                    Quantity = detailDto.Quantity,
                    UnitPrice = detailDto.UnitPrice
                };

                order.OrderDetails.Add(orderDetail);
                totalAmount += detailDto.Quantity * detailDto.UnitPrice;

                // Update book quantity
                book.Quantity -= detailDto.Quantity;
                await _bookRepository.UpdateAsync(book);
            }

            // Set subtotal and calculate final amount
            order.SubTotal = totalAmount;
            order.TotalAmount = Math.Max(0, totalAmount - createOrderDto.VoucherDiscount + createOrderDto.ShippingFee);

            // If voucher is used, find and link the voucher
            if (!string.IsNullOrEmpty(createOrderDto.VoucherCode))
            {
                var voucher = await _context.Vouchers
                    .FirstOrDefaultAsync(v => v.Code == createOrderDto.VoucherCode.ToUpper());
                if (voucher != null)
                {
                    order.VoucherId = voucher.Id;

                    // Create voucher usage record
                    var voucherUsage = new VoucherUsage
                    {
                        VoucherId = voucher.Id,
                        UserId = createOrderDto.UserId,
                        OrderId = 0, // Will be set after order is created
                        UsedAt = DateTime.UtcNow,
                        DiscountAmount = createOrderDto.VoucherDiscount
                    };

                    // Increment voucher used count
                    voucher.UsedCount++;
                    await _context.SaveChangesAsync();
                }
            }
            var createdOrder = await _orderRepository.AddAsync(order);

            // Get the order with details for return
            var orderWithDetails = await _orderRepository.GetOrderWithDetailsAsync(createdOrder.Id);
            var orderDto = _mapper.Map<OrderDto>(orderWithDetails);

            // Send order confirmation email
            try
            {
                var user = await _userRepository.GetByIdAsync(createOrderDto.UserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    var customerName = $"{user.FirstName} {user.LastName}".Trim();
                    if (string.IsNullOrEmpty(customerName))
                        customerName = user.Username;

                    await _emailService.SendOrderConfirmationEmailAsync(user.Email, orderDto, customerName);
                }
            }
            catch (Exception ex)
            {
                // Log email error but don't fail the order creation
                Console.WriteLine($"Failed to send order confirmation email: {ex.Message}");
            }

            return orderDto;
        }

        public async Task<OrderDto?> UpdateOrderAsync(int id, UpdateOrderDto updateOrderDto)
        {
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return null;

            order.Status = updateOrderDto.Status;
            order.ShippingAddress = updateOrderDto.ShippingAddress;
            order.PaymentMethod = updateOrderDto.PaymentMethod;
            order.UpdatedAt = DateTime.UtcNow;

            await _orderRepository.UpdateAsync(order);
            
            var updatedOrder = await _orderRepository.GetOrderWithDetailsAsync(id);
            return _mapper.Map<OrderDto>(updatedOrder);
        }

        public async Task<bool> DeleteOrderAsync(int id)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(id);
            if (order == null)
                return false;

            // Restore book quantities if order is being deleted
            foreach (var detail in order.OrderDetails)
            {
                var book = await _bookRepository.GetByIdAsync(detail.BookId);
                if (book != null)
                {
                    book.Quantity += detail.Quantity;
                    await _bookRepository.UpdateAsync(book);
                }
            }

            await _orderRepository.DeleteAsync(order);
            return true;
        }

        public async Task<OrderStatisticsDto> GetOrderStatisticsAsync()
        {
            try
            {
                var statusCounts = await _orderRepository.GetOrderCountByStatusAsync();
                var totalRevenue = await _orderRepository.GetTotalRevenueAsync();
                var currentDate = DateTime.Now;
                var monthlyRevenue = await _orderRepository.GetMonthlyRevenueAsync(currentDate.Year, currentDate.Month);
                var dailyRevenue = await _orderRepository.GetDailyRevenueAsync(currentDate);

                // Get monthly revenue for the last 12 months
                var monthlyRevenueChart = new List<MonthlyRevenueDto>();
                for (int i = 11; i >= 0; i--)
                {
                    var date = currentDate.AddMonths(-i);
                    var revenue = await _orderRepository.GetMonthlyRevenueAsync(date.Year, date.Month);
                    var orders = await _orderRepository.GetOrdersByDateRangeAsync(
                        new DateTime(date.Year, date.Month, 1),
                        new DateTime(date.Year, date.Month, DateTime.DaysInMonth(date.Year, date.Month)));

                    monthlyRevenueChart.Add(new MonthlyRevenueDto
                    {
                        Year = date.Year,
                        Month = date.Month,
                        MonthName = date.ToString("MM/yyyy"),
                        Revenue = revenue,
                        OrderCount = orders.Count()
                    });
                }

                return new OrderStatisticsDto
                {
                    TotalOrders = statusCounts.Values.Sum(),
                    PendingOrders = statusCounts.GetValueOrDefault("Pending", 0),
                    ProcessingOrders = statusCounts.GetValueOrDefault("Processing", 0),
                    CompletedOrders = statusCounts.GetValueOrDefault("Completed", 0),
                    CancelledOrders = statusCounts.GetValueOrDefault("Cancelled", 0),
                    TotalRevenue = totalRevenue,
                    MonthlyRevenue = monthlyRevenue,
                    DailyRevenue = dailyRevenue,
                    MonthlyRevenueChart = monthlyRevenueChart
                };
            }
            catch (Exception)
            {
                return new OrderStatisticsDto();
            }
        }

        public async Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(string status)
        {
            var orders = await _orderRepository.GetOrdersByStatusAsync(status);
            return _mapper.Map<IEnumerable<OrderDto>>(orders);
        }

        public async Task<bool> UpdateOrderStatusAsync(int id, string status)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(id);
            if (order == null)
                return false;

            var oldStatus = order.Status;
            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            // Send status update email
            try
            {
                var user = await _userRepository.GetByIdAsync(order.UserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    var customerName = $"{user.FirstName} {user.LastName}".Trim();
                    if (string.IsNullOrEmpty(customerName))
                        customerName = user.Username;

                    var orderDto = _mapper.Map<OrderDto>(order);
                    await _emailService.SendOrderStatusUpdateEmailAsync(user.Email, orderDto, customerName, oldStatus, status);
                }
            }
            catch (Exception ex)
            {
                // Log email error but don't fail the status update
                Console.WriteLine($"Failed to send order status update email: {ex.Message}");
            }

            return true;
        }

        public async Task<bool> CancelOrderAsync(int id, string cancellationReason)
        {
            var order = await _orderRepository.GetOrderWithDetailsAsync(id);
            if (order == null)
                return false;

            // Only allow cancellation of Pending or Confirmed orders
            if (order.Status != "Pending" && order.Status != "Confirmed")
                return false;

            order.Status = "Cancelled";
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);

            // Restore book quantities
            foreach (var detail in order.OrderDetails)
            {
                var book = await _bookRepository.GetByIdAsync(detail.BookId);
                if (book != null)
                {
                    book.Quantity += detail.Quantity;
                    await _bookRepository.UpdateAsync(book);
                }
            }

            // Send cancellation email
            try
            {
                var user = await _userRepository.GetByIdAsync(order.UserId);
                if (user != null && !string.IsNullOrEmpty(user.Email))
                {
                    var customerName = $"{user.FirstName} {user.LastName}".Trim();
                    if (string.IsNullOrEmpty(customerName))
                        customerName = user.Username;

                    var orderDto = _mapper.Map<OrderDto>(order);
                    await _emailService.SendOrderCancellationEmailAsync(user.Email, orderDto, customerName, cancellationReason);
                }
            }
            catch (Exception ex)
            {
                // Log email error but don't fail the cancellation
                Console.WriteLine($"Failed to send order cancellation email: {ex.Message}");
            }

            return true;
        }

        public async Task<ReorderResultDto> ReorderAsync(int orderId, int userId)
        {
            try
            {
                // Get the original order
                var originalOrder = await _orderRepository.GetOrderWithDetailsAsync(orderId);
                if (originalOrder == null || originalOrder.UserId != userId)
                {
                    return new ReorderResultDto
                    {
                        Success = false,
                        Message = "Không tìm thấy đơn hàng hoặc bạn không có quyền truy cập"
                    };
                }

                var reorderItems = new List<ReorderItemDto>();
                var unavailableItems = new List<string>();
                var priceChangedItems = new List<string>();
                decimal totalAmount = 0;

                // Check each item in the original order
                foreach (var orderDetail in originalOrder.OrderDetails)
                {
                    var currentBook = await _bookRepository.GetByIdAsync(orderDetail.BookId);

                    if (currentBook == null)
                    {
                        unavailableItems.Add($"{orderDetail.Book?.Title ?? "Sách không xác định"} - Sách không còn tồn tại");
                        continue;
                    }

                    if (currentBook.Quantity < orderDetail.Quantity)
                    {
                        if (currentBook.Quantity > 0)
                        {
                            // Add available quantity
                            reorderItems.Add(new ReorderItemDto
                            {
                                BookId = orderDetail.BookId,
                                BookTitle = currentBook.Title,
                                OriginalQuantity = orderDetail.Quantity,
                                AvailableQuantity = currentBook.Quantity,
                                OriginalPrice = orderDetail.UnitPrice,
                                CurrentPrice = currentBook.Price
                            });

                            totalAmount += currentBook.Price * currentBook.Quantity;

                            if (orderDetail.UnitPrice != currentBook.Price)
                            {
                                priceChangedItems.Add($"{currentBook.Title} - Giá từ {orderDetail.UnitPrice:N0}đ thành {currentBook.Price:N0}đ");
                            }

                            unavailableItems.Add($"{currentBook.Title} - Chỉ còn {currentBook.Quantity} cuốn (bạn đã đặt {orderDetail.Quantity} cuốn)");
                        }
                        else
                        {
                            unavailableItems.Add($"{currentBook.Title} - Hết hàng");
                        }
                    }
                    else
                    {
                        // Item is available with requested quantity
                        reorderItems.Add(new ReorderItemDto
                        {
                            BookId = orderDetail.BookId,
                            BookTitle = currentBook.Title,
                            OriginalQuantity = orderDetail.Quantity,
                            AvailableQuantity = orderDetail.Quantity,
                            OriginalPrice = orderDetail.UnitPrice,
                            CurrentPrice = currentBook.Price
                        });

                        totalAmount += currentBook.Price * orderDetail.Quantity;

                        if (orderDetail.UnitPrice != currentBook.Price)
                        {
                            priceChangedItems.Add($"{currentBook.Title} - Giá từ {orderDetail.UnitPrice:N0}đ thành {currentBook.Price:N0}đ");
                        }
                    }
                }

                return new ReorderResultDto
                {
                    Success = true,
                    Message = reorderItems.Any() ? "Đã chuẩn bị danh sách sản phẩm để đặt lại" : "Không có sản phẩm nào có thể đặt lại",
                    OriginalOrderId = orderId,
                    ReorderItems = reorderItems,
                    UnavailableItems = unavailableItems,
                    PriceChangedItems = priceChangedItems,
                    TotalAmount = totalAmount,
                    OriginalTotalAmount = originalOrder.TotalAmount
                };
            }
            catch (Exception ex)
            {
                return new ReorderResultDto
                {
                    Success = false,
                    Message = $"Có lỗi xảy ra khi chuẩn bị đặt lại đơn hàng: {ex.Message}"
                };
            }
        }
    }
}
