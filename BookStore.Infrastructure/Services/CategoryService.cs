using AutoMapper;
using BookStore.Core.DTOs;
using BookStore.Core.Entities;
using BookStore.Core.Interfaces;

namespace BookStore.Infrastructure.Services
{
    public class CategoryService : ICategoryService
    {
        private readonly ICategoryRepository _categoryRepository;
        private readonly IMapper _mapper;

        public CategoryService(ICategoryRepository categoryRepository, IMapper mapper)
        {
            _categoryRepository = categoryRepository;
            _mapper = mapper;
        }

        public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryDto categoryDto)
        {
            // Check if category with the same name already exists
            var existingCategory = await _categoryRepository.GetByNameAsync(categoryDto.Name);
            if (existingCategory != null)
            {
                throw new Exception($"Category with name '{categoryDto.Name}' already exists");
            }

            var category = _mapper.Map<Category>(categoryDto);
            var createdCategory = await _categoryRepository.AddAsync(category);
            
            var result = _mapper.Map<CategoryDto>(createdCategory);
            result.BookCount = 0; // New category has no books
            
            return result;
        }

        public async Task<bool> DeleteCategoryAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return false;
            }

            // Check if category has books
            if (category.Books.Any())
            {
                throw new Exception("Cannot delete category that has books");
            }

            await _categoryRepository.DeleteAsync(category);
            return true;
        }

        public async Task<IEnumerable<CategoryDto>> GetAllCategoriesAsync()
        {
            var categories = await _categoryRepository.GetAllAsync();
            var categoriesDto = _mapper.Map<IEnumerable<CategoryDto>>(categories);
            
            // Set book count for each category
            foreach (var categoryDto in categoriesDto)
            {
                var category = categories.First(c => c.Id == categoryDto.Id);
                categoryDto.BookCount = category.Books.Count;
            }
            
            return categoriesDto;
        }

        public async Task<CategoryDto?> GetCategoryByIdAsync(int id)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return null;
            }
            
            var categoryDto = _mapper.Map<CategoryDto>(category);
            categoryDto.BookCount = category.Books.Count;
            
            return categoryDto;
        }

        public async Task<CategoryDto?> UpdateCategoryAsync(int id, UpdateCategoryDto categoryDto)
        {
            var category = await _categoryRepository.GetByIdAsync(id);
            if (category == null)
            {
                return null;
            }

            // Check if another category with the same name already exists
            var existingCategory = await _categoryRepository.GetByNameAsync(categoryDto.Name);
            if (existingCategory != null && existingCategory.Id != id)
            {
                throw new Exception($"Category with name '{categoryDto.Name}' already exists");
            }

            _mapper.Map(categoryDto, category);
            await _categoryRepository.UpdateAsync(category);
            
            var result = _mapper.Map<CategoryDto>(category);
            result.BookCount = category.Books.Count;
            
            return result;
        }
    }
} 