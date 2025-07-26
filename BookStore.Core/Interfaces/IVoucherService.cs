using BookStore.Core.DTOs;

namespace BookStore.Core.Interfaces
{
    public interface IVoucherService
    {
        Task<IEnumerable<VoucherDto>> GetAllVouchersAsync();
        Task<VoucherDto?> GetVoucherByIdAsync(int id);
        Task<VoucherDto?> GetVoucherByCodeAsync(string code);
        Task<VoucherDto> CreateVoucherAsync(CreateVoucherDto createVoucherDto);
        Task<VoucherDto> UpdateVoucherAsync(int id, UpdateVoucherDto updateVoucherDto);
        Task<bool> DeleteVoucherAsync(int id);
        
        // Voucher validation and application
        Task<VoucherValidationResultDto> ValidateVoucherAsync(VoucherValidationDto validationDto);
        Task<VoucherValidationResultDto> ApplyVoucherAsync(ApplyVoucherDto applyVoucherDto);
        
        // Usage tracking
        Task<IEnumerable<VoucherUsageDto>> GetVoucherUsagesAsync(int voucherId);
        Task<IEnumerable<VoucherUsageDto>> GetUserVoucherUsagesAsync(int userId);
        Task<int> GetUserVoucherUsageCountAsync(int userId, int voucherId);
        
        // Admin functions
        Task<IEnumerable<VoucherDto>> GetActiveVouchersAsync();
        Task<IEnumerable<VoucherDto>> GetExpiredVouchersAsync();
        Task<bool> DeactivateVoucherAsync(int id);
        Task<bool> ActivateVoucherAsync(int id);
    }
}
