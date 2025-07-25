using BookStore.Core.Entities;

namespace BookStore.Core.Interfaces
{
    public interface IBannerRepository : IRepository<Banner>
    {
        Task<IEnumerable<Banner>> GetActiveBannersAsync();
        Task<IEnumerable<Banner>> GetBannersByPositionAsync(string position);
        Task<IEnumerable<Banner>> GetBannersByDisplayOrderAsync();
        Task<Banner?> GetBannerByIdAsync(int id);
        Task<bool> UpdateDisplayOrderAsync(int id, int newOrder);
        Task<bool> ToggleActiveStatusAsync(int id);
        Task<IEnumerable<Banner>> GetActiveBannersByPositionAsync(string position);
    }
}
