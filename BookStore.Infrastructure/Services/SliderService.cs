using AutoMapper;
using BookStore.Core.DTOs;
using BookStore.Core.Entities;
using BookStore.Core.Interfaces;

namespace BookStore.Infrastructure.Services
{
    public class SliderService : ISliderService
    {
        private readonly ISliderRepository _sliderRepository;
        private readonly IMapper _mapper;

        public SliderService(ISliderRepository sliderRepository, IMapper mapper)
        {
            _sliderRepository = sliderRepository;
            _mapper = mapper;
        }

        public async Task<IEnumerable<SliderDto>> GetAllSlidersAsync()
        {
            var sliders = await _sliderRepository.GetSlidersByDisplayOrderAsync();
            return _mapper.Map<IEnumerable<SliderDto>>(sliders);
        }

        public async Task<IEnumerable<SliderDto>> GetActiveSlidersAsync()
        {
            var sliders = await _sliderRepository.GetActiveSlidersAsync();
            return _mapper.Map<IEnumerable<SliderDto>>(sliders);
        }

        public async Task<SliderDto?> GetSliderByIdAsync(int id)
        {
            var slider = await _sliderRepository.GetSliderByIdAsync(id);
            return slider == null ? null : _mapper.Map<SliderDto>(slider);
        }

        public async Task<SliderDto> CreateSliderAsync(CreateSliderDto createSliderDto)
        {
            var slider = _mapper.Map<Slider>(createSliderDto);
            slider.CreatedAt = DateTime.UtcNow;
            
            await _sliderRepository.AddAsync(slider);
            return _mapper.Map<SliderDto>(slider);
        }

        public async Task<SliderDto?> UpdateSliderAsync(int id, UpdateSliderDto updateSliderDto)
        {
            var slider = await _sliderRepository.GetSliderByIdAsync(id);
            if (slider == null) return null;

            _mapper.Map(updateSliderDto, slider);
            slider.UpdatedAt = DateTime.UtcNow;
            
            await _sliderRepository.UpdateAsync(slider);
            return _mapper.Map<SliderDto>(slider);
        }

        public async Task<bool> DeleteSliderAsync(int id)
        {
            var slider = await _sliderRepository.GetSliderByIdAsync(id);
            if (slider == null) return false;

            await _sliderRepository.DeleteAsync(slider);
            return true;
        }

        public async Task<bool> UpdateDisplayOrderAsync(int id, int newOrder)
        {
            return await _sliderRepository.UpdateDisplayOrderAsync(id, newOrder);
        }

        public async Task<bool> ToggleActiveStatusAsync(int id)
        {
            return await _sliderRepository.ToggleActiveStatusAsync(id);
        }
    }
}
