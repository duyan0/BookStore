namespace BookStore.Core.DTOs
{
    public class OrderDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string UserFullName { get; set; } = string.Empty;
        public DateTime OrderDate { get; set; }
        public decimal TotalAmount { get; set; }
        public string Status { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public List<OrderDetailDto> OrderDetails { get; set; } = new List<OrderDetailDto>();
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class OrderDetailDto
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookImageUrl { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal TotalPrice => Quantity * UnitPrice;
    }

    public class CreateOrderDto
    {
        public int UserId { get; set; }
        public string ShippingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
        public List<CreateOrderDetailDto> OrderDetails { get; set; } = new List<CreateOrderDetailDto>();
    }

    public class CreateOrderDetailDto
    {
        public int BookId { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }

    public class UpdateOrderDto
    {
        public string Status { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string PaymentMethod { get; set; } = string.Empty;
    }

    public class OrderStatisticsDto
    {
        public int TotalOrders { get; set; }
        public int PendingOrders { get; set; }
        public int ProcessingOrders { get; set; }
        public int CompletedOrders { get; set; }
        public int CancelledOrders { get; set; }
        public decimal TotalRevenue { get; set; }
        public decimal MonthlyRevenue { get; set; }
        public decimal DailyRevenue { get; set; }
        public List<MonthlyRevenueDto> MonthlyRevenueChart { get; set; } = new List<MonthlyRevenueDto>();
    }

    public class MonthlyRevenueDto
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public string MonthName { get; set; } = string.Empty;
        public decimal Revenue { get; set; }
        public int OrderCount { get; set; }
    }

    public class ReorderResultDto
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public int OriginalOrderId { get; set; }
        public List<ReorderItemDto> ReorderItems { get; set; } = new List<ReorderItemDto>();
        public List<string> UnavailableItems { get; set; } = new List<string>();
        public List<string> PriceChangedItems { get; set; } = new List<string>();
        public decimal TotalAmount { get; set; }
        public decimal OriginalTotalAmount { get; set; }
    }

    public class ReorderItemDto
    {
        public int BookId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public int OriginalQuantity { get; set; }
        public int AvailableQuantity { get; set; }
        public decimal OriginalPrice { get; set; }
        public decimal CurrentPrice { get; set; }
    }
}
