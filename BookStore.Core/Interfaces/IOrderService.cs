using BookStore.Core.DTOs;

namespace BookStore.Core.Interfaces
{
    public interface IOrderService
    {
        Task<IEnumerable<OrderDto>> GetAllOrdersAsync();
        Task<IEnumerable<OrderDto>> GetOrdersByUserIdAsync(int userId);
        Task<OrderDto?> GetOrderByIdAsync(int id);
        Task<OrderDto> CreateOrderAsync(CreateOrderDto createOrderDto);
        Task<OrderDto?> UpdateOrderAsync(int id, UpdateOrderDto updateOrderDto);
        Task<bool> DeleteOrderAsync(int id);
        Task<OrderStatisticsDto> GetOrderStatisticsAsync();
        Task<IEnumerable<OrderDto>> GetOrdersByStatusAsync(string status);
        Task<bool> UpdateOrderStatusAsync(int id, string status);
        Task<bool> CancelOrderAsync(int id, string cancellationReason);
        Task<ReorderResultDto> ReorderAsync(int orderId, int userId);
    }
}
