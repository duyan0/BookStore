using AutoMapper;
using BookStore.Core.DTOs;
using BookStore.Core.Entities;
using BookStore.Core.Interfaces;

namespace BookStore.Infrastructure.Services
{
    public class BookService : IBookService
    {
        private readonly IBookRepository _bookRepository;
        private readonly ICategoryRepository _categoryRepository;
        private readonly IAuthorRepository _authorRepository;
        private readonly IMapper _mapper;

        public BookService(
            IBookRepository bookRepository,
            ICategoryRepository categoryRepository,
            IAuthorRepository authorRepository,
            IMapper mapper)
        {
            _bookRepository = bookRepository;
            _categoryRepository = categoryRepository;
            _authorRepository = authorRepository;
            _mapper = mapper;
        }

        public async Task<BookDto> CreateBookAsync(CreateBookDto bookDto)
        {
            // Check if category and author exist
            var categoryExists = await _categoryRepository.ExistsAsync(bookDto.CategoryId);
            if (!categoryExists)
            {
                throw new Exception($"Category with ID {bookDto.CategoryId} not found");
            }

            var authorExists = await _authorRepository.ExistsAsync(bookDto.AuthorId);
            if (!authorExists)
            {
                throw new Exception($"Author with ID {bookDto.AuthorId} not found");
            }

            var book = _mapper.Map<Book>(bookDto);
            var createdBook = await _bookRepository.AddAsync(book);
            return _mapper.Map<BookDto>(createdBook);
        }

        public async Task<bool> DeleteBookAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
            {
                return false;
            }

            await _bookRepository.DeleteAsync(book);
            return true;
        }

        public async Task<IEnumerable<BookDto>> GetAllBooksAsync()
        {
            var books = await _bookRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<BookDto>>(books);
        }

        public async Task<IEnumerable<BookDto>> GetBooksByAuthorIdAsync(int authorId)
        {
            var books = await _bookRepository.GetBooksByAuthorIdAsync(authorId);
            return _mapper.Map<IEnumerable<BookDto>>(books);
        }

        public async Task<IEnumerable<BookDto>> GetBooksByCategoryIdAsync(int categoryId)
        {
            var books = await _bookRepository.GetBooksByCategoryIdAsync(categoryId);
            return _mapper.Map<IEnumerable<BookDto>>(books);
        }

        public async Task<BookDto?> GetBookByIdAsync(int id)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            return book != null ? _mapper.Map<BookDto>(book) : null;
        }

        public async Task<IEnumerable<BookDto>> SearchBooksAsync(string searchTerm)
        {
            var books = await _bookRepository.SearchBooksAsync(searchTerm);
            return _mapper.Map<IEnumerable<BookDto>>(books);
        }

        public async Task<BookDto?> UpdateBookAsync(int id, UpdateBookDto bookDto)
        {
            var book = await _bookRepository.GetByIdAsync(id);
            if (book == null)
            {
                return null;
            }

            // Check if category and author exist
            var categoryExists = await _categoryRepository.ExistsAsync(bookDto.CategoryId);
            if (!categoryExists)
            {
                throw new Exception($"Category with ID {bookDto.CategoryId} not found");
            }

            var authorExists = await _authorRepository.ExistsAsync(bookDto.AuthorId);
            if (!authorExists)
            {
                throw new Exception($"Author with ID {bookDto.AuthorId} not found");
            }

            _mapper.Map(bookDto, book);
            await _bookRepository.UpdateAsync(book);
            return _mapper.Map<BookDto>(book);
        }

        public async Task<BookStatisticsDto> GetBookStatisticsAsync()
        {
            try
            {
                var books = await _bookRepository.GetAllAsync();
                var categories = await _categoryRepository.GetAllAsync();
                var authors = await _authorRepository.GetAllAsync();

                var booksList = books.ToList();
                var categoriesList = categories.ToList();
                var authorsList = authors.ToList();

                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfWeek = now.AddDays(-(int)now.DayOfWeek);

                // Calculate statistics
                var totalBooks = booksList.Count;
                var booksInStock = booksList.Count(b => b.Quantity > 0);
                var booksOutOfStock = booksList.Count(b => b.Quantity == 0);
                var lowStockBooks = booksList.Count(b => b.Quantity > 0 && b.Quantity < 10);
                var newBooksThisMonth = booksList.Count(b => b.CreatedAt >= startOfMonth);
                var newBooksThisWeek = booksList.Count(b => b.CreatedAt >= startOfWeek);
                var totalInventoryValue = booksList.Sum(b => b.Price * b.Quantity);
                var averageBookPrice = booksList.Any() ? booksList.Average(b => b.Price) : 0;

                // Top categories by book count
                var topCategories = categoriesList.Select(c => new CategoryStatDto
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    BookCount = booksList.Count(b => b.CategoryId == c.Id),
                    TotalValue = booksList.Where(b => b.CategoryId == c.Id).Sum(b => b.Price * b.Quantity)
                }).OrderByDescending(c => c.BookCount).Take(5).ToList();

                // Recent books (last 10)
                var recentBooks = booksList
                    .OrderByDescending(b => b.CreatedAt)
                    .Take(10)
                    .Select(b => _mapper.Map<BookDto>(b))
                    .ToList();

                return new BookStatisticsDto
                {
                    TotalBooks = totalBooks,
                    BooksInStock = booksInStock,
                    BooksOutOfStock = booksOutOfStock,
                    LowStockBooks = lowStockBooks,
                    NewBooksThisMonth = newBooksThisMonth,
                    NewBooksThisWeek = newBooksThisWeek,
                    TotalInventoryValue = totalInventoryValue,
                    AverageBookPrice = averageBookPrice,
                    TotalCategories = categoriesList.Count,
                    TotalAuthors = authorsList.Count,
                    TopCategories = topCategories,
                    RecentBooks = recentBooks,
                    LastUpdated = DateTime.UtcNow
                };
            }
            catch (Exception)
            {
                return new BookStatisticsDto
                {
                    LastUpdated = DateTime.UtcNow
                };
            }
        }
    }
}