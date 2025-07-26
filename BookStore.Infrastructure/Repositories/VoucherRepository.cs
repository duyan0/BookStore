using BookStore.Core.Entities;
using BookStore.Core.Interfaces;
using BookStore.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace BookStore.Infrastructure.Repositories
{
    public class VoucherRepository : BaseRepository<Voucher>, IVoucherRepository
    {
        public VoucherRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Voucher?> GetByCodeAsync(string code)
        {
            return await _dbSet
                .FirstOrDefaultAsync(v => v.Code.ToUpper() == code.ToUpper());
        }

        public async Task<IEnumerable<Voucher>> GetActiveVouchersAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(v => v.IsActive && 
                           v.StartDate <= now && 
                           v.EndDate >= now &&
                           (v.UsageLimit == null || v.UsedCount < v.UsageLimit))
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<Voucher>> GetExpiredVouchersAsync()
        {
            var now = DateTime.UtcNow;
            return await _dbSet
                .Where(v => v.EndDate < now || (v.UsageLimit.HasValue && v.UsedCount >= v.UsageLimit))
                .OrderByDescending(v => v.EndDate)
                .ToListAsync();
        }

        public async Task<IEnumerable<Voucher>> GetVouchersByTypeAsync(VoucherType type)
        {
            return await _dbSet
                .Where(v => v.Type == type)
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }

        public async Task<bool> IsCodeUniqueAsync(string code, int? excludeId = null)
        {
            var query = _dbSet.Where(v => v.Code.ToUpper() == code.ToUpper());
            
            if (excludeId.HasValue)
            {
                query = query.Where(v => v.Id != excludeId.Value);
            }

            return !await query.AnyAsync();
        }

        public new async Task<IEnumerable<Voucher>> GetAllAsync()
        {
            return await _dbSet
                .OrderByDescending(v => v.CreatedAt)
                .ToListAsync();
        }
    }
}
