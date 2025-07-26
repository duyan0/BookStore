using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookStore.Core.DTOs;
using BookStore.Core.Interfaces;

namespace BookStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private readonly IOrderService _orderService;
        private readonly ILogger<OrdersController> _logger;

        public OrdersController(IOrderService orderService, ILogger<OrdersController> logger)
        {
            _orderService = orderService;
            _logger = logger;
        }

        // GET: api/Orders
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrders()
        {
            var orders = await _orderService.GetAllOrdersAsync();
            return Ok(orders);
        }

        // GET: api/orders/user/{userId}
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetUserOrders(int userId)
        {
            try
            {
                // Users can only view their own orders, admins can view all
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Admin");

                if (!isAdmin && userId.ToString() != currentUserId)
                {
                    return Forbid();
                }

                var orders = await _orderService.GetOrdersByUserIdAsync(userId);
                return Ok(orders);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving orders for user {UserId}", userId);
                return StatusCode(500, "Internal server error");
            }
        }

        // GET: api/Orders/5
        [HttpGet("{id}")]
        public async Task<ActionResult<OrderDto>> GetOrder(int id)
        {
            var order = await _orderService.GetOrderByIdAsync(id);

            if (order == null)
            {
                return NotFound();
            }

            // Users can only view their own orders, admins can view all
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");
            
            if (!isAdmin && order.UserId.ToString() != currentUserId)
            {
                return Forbid();
            }

            return Ok(order);
        }



        // GET: api/Orders/status/pending
        [HttpGet("status/{status}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<OrderDto>>> GetOrdersByStatus(string status)
        {
            var orders = await _orderService.GetOrdersByStatusAsync(status);
            return Ok(orders);
        }

        // POST: api/Orders/reorder/5
        [HttpPost("reorder/{id}")]
        [Authorize]
        public async Task<ActionResult<ReorderResultDto>> ReorderOrder(int id)
        {
            try
            {
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(currentUserId))
                {
                    return Unauthorized();
                }

                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound(new { message = "Không tìm thấy đơn hàng" });
                }

                // Check if user owns this order
                if (order.UserId.ToString() != currentUserId)
                {
                    return Forbid("Bạn không có quyền truy cập đơn hàng này");
                }

                // Check if order is completed
                if (order.Status != "Completed")
                {
                    return BadRequest(new { message = "Chỉ có thể mua lại đơn hàng đã hoàn thành" });
                }

                var result = await _orderService.ReorderAsync(id, int.Parse(currentUserId));
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error reordering order {OrderId}", id);
                return StatusCode(500, new { message = "Có lỗi xảy ra khi mua lại đơn hàng" });
            }
        }

        // GET: api/Orders/statistics
        [HttpGet("statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<OrderStatisticsDto>> GetOrderStatistics()
        {
            var statistics = await _orderService.GetOrderStatisticsAsync();
            return Ok(statistics);
        }

        // POST: api/Orders
        [HttpPost]
        public async Task<ActionResult<OrderDto>> CreateOrder(CreateOrderDto createOrderDto)
        {
            try
            {
                // Ensure user can only create orders for themselves (unless admin)
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Admin");
                
                if (!isAdmin && createOrderDto.UserId.ToString() != currentUserId)
                {
                    return Forbid();
                }

                var order = await _orderService.CreateOrderAsync(createOrderDto);
                return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "An error occurred while creating the order", details = ex.Message });
            }
        }

        // PUT: api/Orders/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrder(int id, UpdateOrderDto updateOrderDto)
        {
            try
            {
                var updatedOrder = await _orderService.UpdateOrderAsync(id, updateOrderDto);

                if (updatedOrder == null)
                {
                    return NotFound();
                }

                return Ok(updatedOrder);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Orders/5/status
        [HttpPut("{id}/status")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateOrderStatus(int id, [FromBody] string status)
        {
            try
            {
                var result = await _orderService.UpdateOrderStatusAsync(id, status);

                if (!result)
                {
                    return NotFound();
                }

                return Ok(new { message = "Order status updated successfully" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Orders/5/cancel
        [HttpPut("{id}/cancel")]
        [Authorize]
        public async Task<IActionResult> CancelOrder(int id, [FromBody] CancelOrderDto cancelDto)
        {
            try
            {
                // Get current user ID from token
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Admin");

                // Check if user can cancel this order
                var order = await _orderService.GetOrderByIdAsync(id);
                if (order == null)
                {
                    return NotFound();
                }

                if (!isAdmin && order.UserId.ToString() != currentUserId)
                {
                    return Forbid();
                }

                var result = await _orderService.CancelOrderAsync(id, cancelDto.CancellationReason);

                if (!result)
                {
                    return BadRequest(new { message = "Không thể hủy đơn hàng này. Đơn hàng có thể đã được xử lý hoặc hoàn thành." });
                }

                return Ok(new { message = "Đơn hàng đã được hủy thành công" });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/Orders/test-email-notifications
        [HttpPost("test-email-notifications")]
        [AllowAnonymous]
        public async Task<IActionResult> TestEmailNotifications()
        {
            try
            {
                await BookStore.API.TestEmailNotifications.RunEmailTests(HttpContext.RequestServices);
                return Ok(new { message = "Email notification tests completed. Check console logs for results." });
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/Orders/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteOrder(int id)
        {
            try
            {
                var result = await _orderService.DeleteOrderAsync(id);

                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // POST: api/Orders/{id}/reorder
        [HttpPost("{id}/reorder")]
        [Authorize]
        public async Task<ActionResult<ReorderResultDto>> ReorderAsync(int id)
        {
            try
            {
                // Get current user ID from token
                var userIdClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!int.TryParse(userIdClaim, out int userId))
                {
                    return Unauthorized(new { message = "Invalid user token" });
                }

                var result = await _orderService.ReorderAsync(id, userId);

                if (result.Success)
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error processing reorder", error = ex.Message });
            }
        }
    }
}
