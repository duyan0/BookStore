using BookStore.Core.Entities;

namespace BookStore.Core.Interfaces
{
    public interface IVoucherRepository : IRepository<Voucher>
    {
        Task<Voucher?> GetByCodeAsync(string code);
        Task<IEnumerable<Voucher>> GetActiveVouchersAsync();
        Task<IEnumerable<Voucher>> GetExpiredVouchersAsync();
        Task<IEnumerable<Voucher>> GetVouchersByTypeAsync(VoucherType type);
        Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null);
    }
}
