using BookStore.Core.Entities;

namespace BookStore.Core.Interfaces
{
    public interface ISliderRepository : IRepository<Slider>
    {
        Task<IEnumerable<Slider>> GetActiveSlidersAsync();
        Task<IEnumerable<Slider>> GetSlidersByDisplayOrderAsync();
        Task<Slider?> GetSliderByIdAsync(int id);
        Task<bool> UpdateDisplayOrderAsync(int id, int newOrder);
        Task<bool> ToggleActiveStatusAsync(int id);
    }
}
