using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookStore.Core.DTOs;
using BookStore.Core.Interfaces;

namespace BookStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class VouchersController : ControllerBase
    {
        private readonly IVoucherService _voucherService;
        private readonly ILogger<VouchersController> _logger;

        public VouchersController(IVoucherService voucherService, ILogger<VouchersController> logger)
        {
            _voucherService = voucherService;
            _logger = logger;
        }

        // GET: api/vouchers
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<VoucherDto>>> GetVouchers()
        {
            try
            {
                var vouchers = await _voucherService.GetAllVouchersAsync();
                return Ok(vouchers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vouchers");
                return StatusCode(500, new { message = "Error retrieving vouchers" });
            }
        }

        // GET: api/vouchers/available
        [HttpGet("available")]
        public async Task<ActionResult<IEnumerable<VoucherDto>>> GetAvailableVouchers()
        {
            try
            {
                var vouchers = await _voucherService.GetActiveVouchersAsync();
                return Ok(vouchers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available vouchers");
                return StatusCode(500, new { message = "Error retrieving available vouchers" });
            }
        }

        // GET: api/vouchers/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<VoucherDto>> GetVoucher(int id)
        {
            try
            {
                var voucher = await _voucherService.GetVoucherByIdAsync(id);
                
                if (voucher == null)
                {
                    return NotFound(new { message = "Voucher not found" });
                }

                return Ok(voucher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving voucher {VoucherId}", id);
                return StatusCode(500, new { message = "Error retrieving voucher" });
            }
        }

        // GET: api/vouchers/code/{code}
        [HttpGet("code/{code}")]
        public async Task<ActionResult<VoucherDto>> GetVoucherByCode(string code)
        {
            try
            {
                var voucher = await _voucherService.GetVoucherByCodeAsync(code);

                if (voucher == null)
                {
                    return NotFound(new { message = "Voucher not found" });
                }

                return Ok(voucher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving voucher by code {VoucherCode}", code);
                return StatusCode(500, new { message = "Error retrieving voucher" });
            }
        }

        // GET: api/vouchers/user/{userId}
        [HttpGet("user/{userId}")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<VoucherDto>>> GetUserVouchers(int userId)
        {
            try
            {
                // Ensure user can only access their own vouchers (unless admin)
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Admin");

                if (!isAdmin && userId.ToString() != currentUserId)
                {
                    return Forbid("You can only access your own vouchers");
                }

                var vouchers = await _voucherService.GetUserVouchersAsync(userId);
                return Ok(vouchers);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving vouchers for user {UserId}", userId);
                return StatusCode(500, new { message = "Error retrieving user vouchers" });
            }
        }

        // POST: api/vouchers/validate
        [HttpPost("validate")]
        [AllowAnonymous] // Allow anonymous access for voucher validation
        public async Task<ActionResult<VoucherValidationResultDto>> ValidateVoucher(VoucherValidationDto validationDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var result = await _voucherService.ValidateVoucherAsync(validationDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating voucher {VoucherCode}", validationDto.Code);
                return StatusCode(500, new { message = "Error validating voucher" });
            }
        }

        // POST: api/vouchers/apply
        [HttpPost("apply")]
        [Authorize]
        public async Task<ActionResult<VoucherValidationResultDto>> ApplyVoucher(ApplyVoucherDto applyVoucherDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                // Ensure user can only apply vouchers for themselves (unless admin)
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Admin");
                
                if (!isAdmin && applyVoucherDto.UserId.ToString() != currentUserId)
                {
                    return Forbid("You can only apply vouchers for yourself");
                }

                var result = await _voucherService.ApplyVoucherAsync(applyVoucherDto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error applying voucher {VoucherCode} for user {UserId}", 
                    applyVoucherDto.VoucherCode, applyVoucherDto.UserId);
                return StatusCode(500, new { message = "Error applying voucher" });
            }
        }

        // POST: api/vouchers
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<VoucherDto>> CreateVoucher(CreateVoucherDto createVoucherDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var voucher = await _voucherService.CreateVoucherAsync(createVoucherDto);
                return CreatedAtAction(nameof(GetVoucher), new { id = voucher.Id }, voucher);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating voucher");
                return StatusCode(500, new { message = "Error creating voucher" });
            }
        }

        // PUT: api/vouchers/{id}
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<VoucherDto>> UpdateVoucher(int id, UpdateVoucherDto updateVoucherDto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(ModelState);
                }

                var voucher = await _voucherService.UpdateVoucherAsync(id, updateVoucherDto);
                return Ok(voucher);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating voucher {VoucherId}", id);
                return StatusCode(500, new { message = "Error updating voucher" });
            }
        }

        // DELETE: api/vouchers/{id}
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteVoucher(int id)
        {
            try
            {
                var result = await _voucherService.DeleteVoucherAsync(id);
                
                if (!result)
                {
                    return NotFound(new { message = "Voucher not found" });
                }

                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting voucher {VoucherId}", id);
                return StatusCode(500, new { message = "Error deleting voucher" });
            }
        }

        // POST: api/vouchers/{id}/activate
        [HttpPost("{id}/activate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> ActivateVoucher(int id)
        {
            try
            {
                var voucher = await _voucherService.GetVoucherByIdAsync(id);
                if (voucher == null)
                {
                    return NotFound(new { message = "Voucher not found" });
                }

                if (voucher.IsActive)
                {
                    return BadRequest(new { message = "Voucher is already active" });
                }

                var updateDto = new UpdateVoucherDto
                {
                    Code = voucher.Code,
                    Name = voucher.Name,
                    Description = voucher.Description,
                    Type = voucher.Type,
                    Value = voucher.Value,
                    MinimumOrderAmount = voucher.MinimumOrderAmount,
                    MaximumDiscountAmount = voucher.MaximumDiscountAmount,
                    UsageLimit = voucher.UsageLimit,
                    UsageLimitPerUser = voucher.UsageLimitPerUser,
                    StartDate = voucher.StartDate,
                    EndDate = voucher.EndDate,
                    IsActive = true
                };

                await _voucherService.UpdateVoucherAsync(id, updateDto);
                return Ok(new { message = "Voucher activated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating voucher {VoucherId}", id);
                return StatusCode(500, new { message = "Error activating voucher" });
            }
        }

        // POST: api/vouchers/{id}/deactivate
        [HttpPost("{id}/deactivate")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeactivateVoucher(int id)
        {
            try
            {
                var voucher = await _voucherService.GetVoucherByIdAsync(id);
                if (voucher == null)
                {
                    return NotFound(new { message = "Voucher not found" });
                }

                if (!voucher.IsActive)
                {
                    return BadRequest(new { message = "Voucher is already inactive" });
                }

                var updateDto = new UpdateVoucherDto
                {
                    Code = voucher.Code,
                    Name = voucher.Name,
                    Description = voucher.Description,
                    Type = voucher.Type,
                    Value = voucher.Value,
                    MinimumOrderAmount = voucher.MinimumOrderAmount,
                    MaximumDiscountAmount = voucher.MaximumDiscountAmount,
                    UsageLimit = voucher.UsageLimit,
                    UsageLimitPerUser = voucher.UsageLimitPerUser,
                    StartDate = voucher.StartDate,
                    EndDate = voucher.EndDate,
                    IsActive = false
                };

                await _voucherService.UpdateVoucherAsync(id, updateDto);
                return Ok(new { message = "Voucher deactivated successfully" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating voucher {VoucherId}", id);
                return StatusCode(500, new { message = "Error deactivating voucher" });
            }
        }
    }
}
