using AutoMapper;
using BookStore.Core.DTOs;
using BookStore.Core.Entities;
using BookStore.Core.Interfaces;

namespace BookStore.Infrastructure.Services
{
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _orderRepository;
        private readonly IBookRepository _bookRepository;
        private readonly IMapper _mapper;

        public OrderService(IOrderRepository orderRepository, IBookRepository bookRepository, IMapper mapper)
        {
            _orderRepository = orderRepository;
            _bookRepository = bookRepository;
            _mapper = mapper;
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
                OrderDetails = new List<OrderDetail>()
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

            order.TotalAmount = totalAmount;
            var createdOrder = await _orderRepository.AddAsync(order);
            
            // Get the order with details for return
            var orderWithDetails = await _orderRepository.GetOrderWithDetailsAsync(createdOrder.Id);
            return _mapper.Map<OrderDto>(orderWithDetails);
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
            var order = await _orderRepository.GetByIdAsync(id);
            if (order == null)
                return false;

            order.Status = status;
            order.UpdatedAt = DateTime.UtcNow;
            await _orderRepository.UpdateAsync(order);
            return true;
        }
    }
}
