using BookStore.Core.DTOs;

namespace BookStore.Core.Interfaces
{
    public interface IBannerService
    {
        Task<IEnumerable<BannerDto>> GetAllBannersAsync();
        Task<IEnumerable<BannerDto>> GetActiveBannersAsync();
        Task<IEnumerable<BannerDto>> GetBannersByPositionAsync(string position);
        Task<IEnumerable<BannerDto>> GetActiveBannersByPositionAsync(string position);
        Task<BannerDto?> GetBannerByIdAsync(int id);
        Task<BannerDto> CreateBannerAsync(CreateBannerDto createBannerDto);
        Task<BannerDto?> UpdateBannerAsync(int id, UpdateBannerDto updateBannerDto);
        Task<bool> DeleteBannerAsync(int id);
        Task<bool> UpdateDisplayOrderAsync(int id, int newOrder);
        Task<bool> ToggleActiveStatusAsync(int id);
    }
}
