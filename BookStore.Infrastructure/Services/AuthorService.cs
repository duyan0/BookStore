using AutoMapper;
using BookStore.Core.DTOs;
using BookStore.Core.Entities;
using BookStore.Core.Interfaces;

namespace BookStore.Infrastructure.Services
{
    public class AuthorService : IAuthorService
    {
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;

        public AuthorService(IAuthorRepository authorRepository, IMapper mapper)
        {
            _authorRepository = authorRepository;
            _mapper = mapper;
        }

        public async Task<AuthorDto> CreateAuthorAsync(CreateAuthorDto authorDto)
        {
            var author = _mapper.Map<Author>(authorDto);
            var createdAuthor = await _authorRepository.AddAsync(author);
            
            var result = _mapper.Map<AuthorDto>(createdAuthor);
            result.BookCount = 0; // New author has no books
            
            return result;
        }

        public async Task<bool> DeleteAuthorAsync(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null)
            {
                return false;
            }

            // Check if author has books
            if (author.Books.Any())
            {
                throw new Exception("Cannot delete author that has books");
            }

            await _authorRepository.DeleteAsync(author);
            return true;
        }

        public async Task<IEnumerable<AuthorDto>> GetAllAuthorsAsync()
        {
            var authors = await _authorRepository.GetAllAsync();
            var authorsDto = _mapper.Map<IEnumerable<AuthorDto>>(authors);
            
            // Set book count for each author
            foreach (var authorDto in authorsDto)
            {
                var author = authors.First(a => a.Id == authorDto.Id);
                authorDto.BookCount = author.Books.Count;
            }
            
            return authorsDto;
        }

        public async Task<AuthorDto?> GetAuthorByIdAsync(int id)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null)
            {
                return null;
            }
            
            var authorDto = _mapper.Map<AuthorDto>(author);
            authorDto.BookCount = author.Books.Count;
            
            return authorDto;
        }

        public async Task<IEnumerable<AuthorDto>> SearchAuthorsAsync(string searchTerm)
        {
            var authors = await _authorRepository.SearchAuthorsAsync(searchTerm);
            return _mapper.Map<IEnumerable<AuthorDto>>(authors);
        }

        public async Task<AuthorDto?> UpdateAuthorAsync(int id, UpdateAuthorDto authorDto)
        {
            var author = await _authorRepository.GetByIdAsync(id);
            if (author == null)
            {
                return null;
            }

            _mapper.Map(authorDto, author);
            await _authorRepository.UpdateAsync(author);
            
            var result = _mapper.Map<AuthorDto>(author);
            result.BookCount = author.Books.Count;
            
            return result;
        }
    }
} 