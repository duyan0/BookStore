using BookStore.Core.Entities;

namespace BookStore.Core.Interfaces
{
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status);
        Task<Order?> GetOrderWithDetailsAsync(int id);
        Task<IEnumerable<Order>> GetAllOrdersWithDetailsAsync();
        Task<decimal> GetTotalRevenueAsync();
        Task<decimal> GetMonthlyRevenueAsync(int year, int month);
        Task<decimal> GetDailyRevenueAsync(DateTime date);
        Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate);
        Task<Dictionary<string, int>> GetOrderCountByStatusAsync();
    }
}
