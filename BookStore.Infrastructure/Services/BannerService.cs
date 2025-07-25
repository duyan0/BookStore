using AutoMapper;
using BookStore.Core.DTOs;
using BookStore.Core.Entities;
using BookStore.Core.Interfaces;

namespace BookStore.Infrastructure.Services
{
    public class BannerService : IBannerService
    {
        private readonly IBannerRepository _bannerRepository;
        private readonly IMapper _mapper;

        public BannerService(IBannerRepository bannerRepository, IMapper mapper)
        {
            _bannerRepository = bannerRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<BannerDto>> GetAllBannersAsync()
        {
            var banners = await _bannerRepository.GetBannersByDisplayOrderAsync();
            return _mapper.Map<IEnumerable<BannerDto>>(banners);
        }

        public async Task<IEnumerable<BannerDto>> GetActiveBannersAsync()
        {
            var banners = await _bannerRepository.GetActiveBannersAsync();
            return _mapper.Map<IEnumerable<BannerDto>>(banners);
        }

        public async Task<IEnumerable<BannerDto>> GetBannersByPositionAsync(string position)
        {
            var banners = await _bannerRepository.GetBannersByPositionAsync(position);
            return _mapper.Map<IEnumerable<BannerDto>>(banners);
        }

        public async Task<IEnumerable<BannerDto>> GetActiveBannersByPositionAsync(string position)
        {
            var banners = await _bannerRepository.GetActiveBannersByPositionAsync(position);
            return _mapper.Map<IEnumerable<BannerDto>>(banners);
        }

        public async Task<BannerDto?> GetBannerByIdAsync(int id)
        {
            var banner = await _bannerRepository.GetBannerByIdAsync(id);
            return banner == null ? null : _mapper.Map<BannerDto>(banner);
        }

        public async Task<BannerDto> CreateBannerAsync(CreateBannerDto createBannerDto)
        {
            var banner = _mapper.Map<Banner>(createBannerDto);
            banner.CreatedAt = DateTime.UtcNow;
            
            await _bannerRepository.AddAsync(banner);
            return _mapper.Map<BannerDto>(banner);
        }

        public async Task<BannerDto?> UpdateBannerAsync(int id, UpdateBannerDto updateBannerDto)
        {
            var banner = await _bannerRepository.GetBannerByIdAsync(id);
            if (banner == null) return null;

            _mapper.Map(updateBannerDto, banner);
            banner.UpdatedAt = DateTime.UtcNow;
            
            await _bannerRepository.UpdateAsync(banner);
            return _mapper.Map<BannerDto>(banner);
        }

        public async Task<bool> DeleteBannerAsync(int id)
        {
            var banner = await _bannerRepository.GetBannerByIdAsync(id);
            if (banner == null) return false;

            await _bannerRepository.DeleteAsync(banner);
            return true;
        }

        public async Task<bool> UpdateDisplayOrderAsync(int id, int newOrder)
        {
            return await _bannerRepository.UpdateDisplayOrderAsync(id, newOrder);
        }

        public async Task<bool> ToggleActiveStatusAsync(int id)
        {
            return await _bannerRepository.ToggleActiveStatusAsync(id);
        }
    }
}
