using BookStore.Core.DTOs;

namespace BookStore.Core.Interfaces
{
    public interface IAuthorService
    {
        Task<IEnumerable<AuthorDto>> GetAllAuthorsAsync();
        Task<AuthorDto?> GetAuthorByIdAsync(int id);
        Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto authorDto);
        Task<AuthorDto?> UpdateAuthorAsync(int id, UpdateAuthorDto authorDto);
        Task<bool> DeleteAuthorAsync(int id);
        Task<IEnumerable<AuthorDto>> SearchAuthorsAsync(string searchTerm);
    }
} 