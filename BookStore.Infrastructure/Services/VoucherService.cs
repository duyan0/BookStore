using AutoMapper;
using BookStore.Core.DTOs;
using BookStore.Core.Entities;
using BookStore.Core.Interfaces;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace BookStore.Infrastructure.Services
{
    public class VoucherService : IVoucherService
    {
        private readonly ApplicationDbContext _context;
        private readonly IVoucherRepository _voucherRepository;
        private readonly IMapper _mapper;
        private readonly ILogger<VoucherService> _logger;

        public VoucherService(ApplicationDbContext context, IVoucherRepository voucherRepository, IMapper mapper, ILogger<VoucherService> logger)
        {
            _context = context;
            _voucherRepository = voucherRepository;
            _mapper = mapper;
            _logger = logger;
        }

        public async Task<IEnumerable<VoucherDto>> GetAllVouchersAsync()
        {
            var vouchers = await _context.Vouchers
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<VoucherDto>>(vouchers);
        }

        public async Task<VoucherDto?> GetVoucherByIdAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            return voucher == null ? null : _mapper.Map<VoucherDto>(voucher);
        }

        public async Task<VoucherDto?> GetVoucherByCodeAsync(string code)
        {
            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.Code == code.ToUpper());
            return voucher == null ? null : _mapper.Map<VoucherDto>(voucher);
        }

        public async Task<VoucherDto> CreateVoucherAsync(CreateVoucherDto createVoucherDto)
        {
            // Check if voucher code already exists
            var existingVoucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.Code == createVoucherDto.Code.ToUpper());
            
            if (existingVoucher != null)
            {
                throw new InvalidOperationException($"Voucher với mã '{createVoucherDto.Code}' đã tồn tại");
            }

            // Validate dates
            if (createVoucherDto.EndDate <= createVoucherDto.StartDate)
            {
                throw new ArgumentException("Ngày kết thúc phải sau ngày bắt đầu");
            }

            var voucher = _mapper.Map<Voucher>(createVoucherDto);
            voucher.Code = createVoucherDto.Code.ToUpper(); // Ensure uppercase
            voucher.UsedCount = 0;

            _context.Vouchers.Add(voucher);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Created voucher {VoucherCode} with ID {VoucherId}", voucher.Code, voucher.Id);

            return _mapper.Map<VoucherDto>(voucher);
        }

        public async Task<VoucherDto> UpdateVoucherAsync(int id, UpdateVoucherDto updateVoucherDto)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                throw new ArgumentException($"Không tìm thấy voucher với ID {id}");
            }

            // Validate dates
            if (updateVoucherDto.EndDate <= updateVoucherDto.StartDate)
            {
                throw new ArgumentException("Ngày kết thúc phải sau ngày bắt đầu");
            }

            // Update properties (except Code and UsedCount)
            voucher.Name = updateVoucherDto.Name;
            voucher.Description = updateVoucherDto.Description;
            voucher.Type = updateVoucherDto.Type;
            voucher.Value = updateVoucherDto.Value;
            voucher.MinimumOrderAmount = updateVoucherDto.MinimumOrderAmount;
            voucher.MaximumDiscountAmount = updateVoucherDto.MaximumDiscountAmount;
            voucher.UsageLimit = updateVoucherDto.UsageLimit;
            voucher.UsageLimitPerUser = updateVoucherDto.UsageLimitPerUser;
            voucher.StartDate = updateVoucherDto.StartDate;
            voucher.EndDate = updateVoucherDto.EndDate;
            voucher.IsActive = updateVoucherDto.IsActive;

            await _context.SaveChangesAsync();

            _logger.LogInformation("Updated voucher {VoucherCode} with ID {VoucherId}", voucher.Code, voucher.Id);

            return _mapper.Map<VoucherDto>(voucher);
        }

        public async Task<bool> DeleteVoucherAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                return false;
            }

            // Check if voucher has been used
            var hasUsages = await _context.VoucherUsages.AnyAsync(vu => vu.VoucherId == id);
            if (hasUsages)
            {
                throw new InvalidOperationException("Không thể xóa voucher đã được sử dụng");
            }

            _context.Vouchers.Remove(voucher);
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deleted voucher {VoucherCode} with ID {VoucherId}", voucher.Code, voucher.Id);

            return true;
        }

        public async Task<VoucherValidationResultDto> ValidateVoucherAsync(VoucherValidationDto validationDto)
        {
            var voucher = await _context.Vouchers
                .FirstOrDefaultAsync(v => v.Code == validationDto.Code.ToUpper());

            if (voucher == null)
            {
                return new VoucherValidationResultDto
                {
                    IsValid = false,
                    Message = "Mã voucher không tồn tại"
                };
            }

            if (!voucher.IsValid)
            {
                var reason = voucher.IsExpired ? "đã hết hạn" : 
                           voucher.IsUsageLimitReached ? "đã hết lượt sử dụng" : 
                           "không hoạt động";
                return new VoucherValidationResultDto
                {
                    IsValid = false,
                    Message = $"Mã voucher {reason}"
                };
            }

            if (validationDto.OrderAmount < voucher.MinimumOrderAmount)
            {
                return new VoucherValidationResultDto
                {
                    IsValid = false,
                    Message = $"Đơn hàng tối thiểu {voucher.MinimumOrderAmount:N0} VND để sử dụng voucher này"
                };
            }

            // Check user usage limit
            if (validationDto.UserId.HasValue && voucher.UsageLimitPerUser.HasValue)
            {
                var userUsageCount = await GetUserVoucherUsageCountAsync(validationDto.UserId.Value, voucher.Id);
                if (userUsageCount >= voucher.UsageLimitPerUser.Value)
                {
                    return new VoucherValidationResultDto
                    {
                        IsValid = false,
                        Message = "Bạn đã sử dụng hết lượt cho voucher này"
                    };
                }
            }

            var discountAmount = voucher.CalculateDiscount(validationDto.OrderAmount);
            var freeShipping = voucher.Type == VoucherType.FreeShipping;

            return new VoucherValidationResultDto
            {
                IsValid = true,
                Message = "Voucher hợp lệ",
                DiscountAmount = discountAmount,
                FreeShipping = freeShipping,
                Voucher = _mapper.Map<VoucherDto>(voucher)
            };
        }

        public async Task<VoucherValidationResultDto> ApplyVoucherAsync(ApplyVoucherDto applyVoucherDto)
        {
            var validationResult = await ValidateVoucherAsync(new VoucherValidationDto
            {
                Code = applyVoucherDto.VoucherCode,
                OrderAmount = applyVoucherDto.OrderAmount,
                UserId = applyVoucherDto.UserId
            });

            if (!validationResult.IsValid)
            {
                return validationResult;
            }

            // Voucher is valid, can be applied
            return validationResult;
        }

        public async Task<IEnumerable<VoucherUsageDto>> GetVoucherUsagesAsync(int voucherId)
        {
            var usages = await _context.VoucherUsages
                .Include(vu => vu.Voucher)
                .Include(vu => vu.User)
                .Include(vu => vu.Order)
                .Where(vu => vu.VoucherId == voucherId)
                .OrderByDescending(vu => vu.UsedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<VoucherUsageDto>>(usages);
        }

        public async Task<IEnumerable<VoucherUsageDto>> GetUserVoucherUsagesAsync(int userId)
        {
            var usages = await _context.VoucherUsages
                .Include(vu => vu.Voucher)
                .Include(vu => vu.User)
                .Include(vu => vu.Order)
                .Where(vu => vu.UserId == userId)
                .OrderByDescending(vu => vu.UsedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<VoucherUsageDto>>(usages);
        }

        public async Task<IEnumerable<VoucherDto>> GetUserVouchersAsync(int userId)
        {
            // Get vouchers that the user has used
            var usedVoucherIds = await _context.VoucherUsages
                .Where(vu => vu.UserId == userId)
                .Select(vu => vu.VoucherId)
                .Distinct()
                .ToListAsync();

            var usedVouchers = await _context.Vouchers
                .Where(v => usedVoucherIds.Contains(v.Id))
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();

            return _mapper.Map<IEnumerable<VoucherDto>>(usedVouchers);
        }

        public async Task<int> GetUserVoucherUsageCountAsync(int userId, int voucherId)
        {
            return await _context.VoucherUsages
                .CountAsync(vu => vu.UserId == userId && vu.VoucherId == voucherId);
        }

        public async Task<IEnumerable<VoucherDto>> GetActiveVouchersAsync()
        {
            var now = DateTime.UtcNow;
            var vouchers = await _context.Vouchers
                .Where(v => v.IsActive && 
                           v.StartDate <= now && 
                           v.EndDate >= now &&
                           (v.UsageLimit == null || v.UsedCount < v.UsageLimit))
                .OrderBy(v => v.EndDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<VoucherDto>>(vouchers);
        }

        public async Task<IEnumerable<VoucherDto>> GetExpiredVouchersAsync()
        {
            var now = DateTime.UtcNow;
            var vouchers = await _context.Vouchers
                .Where(v => v.EndDate < now || (v.UsageLimit.HasValue && v.UsedCount >= v.UsageLimit))
                .OrderByDescending(v => v.EndDate)
                .ToListAsync();

            return _mapper.Map<IEnumerable<VoucherDto>>(vouchers);
        }

        public async Task<bool> DeactivateVoucherAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                return false;
            }

            voucher.IsActive = false;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Deactivated voucher {VoucherCode} with ID {VoucherId}", voucher.Code, voucher.Id);

            return true;
        }

        public async Task<bool> ActivateVoucherAsync(int id)
        {
            var voucher = await _context.Vouchers.FindAsync(id);
            if (voucher == null)
            {
                return false;
            }

            voucher.IsActive = true;
            await _context.SaveChangesAsync();

            _logger.LogInformation("Activated voucher {VoucherCode} with ID {VoucherId}", voucher.Code, voucher.Id);

            return true;
        }
    }
}
