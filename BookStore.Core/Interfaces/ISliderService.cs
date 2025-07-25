using BookStore.Core.DTOs;

namespace BookStore.Core.Interfaces
{
    public interface ISliderService
    {
        Task<IEnumerable<SliderDto>> GetAllSlidersAsync();
        Task<IEnumerable<SliderDto>> GetActiveSlidersAsync();
        Task<SliderDto?> GetSliderByIdAsync(int id);
        Task<SliderDto> CreateSliderAsync(CreateSliderDto createSliderDto);
        Task<SliderDto?> UpdateSliderAsync(int id, UpdateSliderDto updateSliderDto);
        Task<bool> DeleteSliderAsync(int id);
        Task<bool> UpdateDisplayOrderAsync(int id, int newOrder);
        Task<bool> ToggleActiveStatusAsync(int id);
    }
}
