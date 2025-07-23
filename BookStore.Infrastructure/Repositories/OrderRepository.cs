using BookStore.Core.Entities;
using BookStore.Core.Interfaces;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Repositories
{
    public class OrderRepository : BaseRepository<Order>, IOrderRepository
    {
        public OrderRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<IEnumerable<Order>> GetOrdersByUserIdAsync(int userId)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .Where(o => o.UserId == userId)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(string status)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .Where(o => o.Status == status)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Order?> GetOrderWithDetailsAsync(int id)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                        .ThenInclude(b => b.Author)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                        .ThenInclude(b => b.Category)
                .FirstOrDefaultAsync(o => o.Id == id);
        }

        public async Task<IEnumerable<Order>> GetAllOrdersWithDetailsAsync()
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<decimal> GetTotalRevenueAsync()
        {
            return await _context.Orders
                .Where(o => o.Status == "Completed")
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<decimal> GetMonthlyRevenueAsync(int year, int month)
        {
            return await _context.Orders
                .Where(o => o.Status == "Completed" && 
                           o.OrderDate.Year == year && 
                           o.OrderDate.Month == month)
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<decimal> GetDailyRevenueAsync(DateTime date)
        {
            return await _context.Orders
                .Where(o => o.Status == "Completed" && 
                           o.OrderDate.Date == date.Date)
                .SumAsync(o => o.TotalAmount);
        }

        public async Task<IEnumerable<Order>> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate)
        {
            return await _context.Orders
                .Include(o => o.User)
                .Include(o => o.OrderDetails)
                    .ThenInclude(od => od.Book)
                .Where(o => o.OrderDate.Date >= startDate.Date && 
                           o.OrderDate.Date <= endDate.Date)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();
        }

        public async Task<Dictionary<string, int>> GetOrderCountByStatusAsync()
        {
            return await _context.Orders
                .GroupBy(o => o.Status)
                .ToDictionaryAsync(g => g.Key, g => g.Count());
        }
    }
}
