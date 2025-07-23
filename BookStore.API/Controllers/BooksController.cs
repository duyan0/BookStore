using BookStore.Core.DTOs;
using BookStore.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BooksController : ControllerBase
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        // GET: api/Books
        [HttpGet]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooks()
        {
            var books = await _bookService.GetAllBooksAsync();
            return Ok(books);
        }

        // GET: api/Books/5
        [HttpGet("{id}")]
        public async Task<ActionResult<BookDto>> GetBook(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);

            if (book == null)
            {
                return NotFound();
            }

            return Ok(book);
        }

        // GET: api/Books/category/5
        [HttpGet("category/{categoryId}")]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooksByCategory(int categoryId)
        {
            var books = await _bookService.GetBooksByCategoryIdAsync(categoryId);
            return Ok(books);
        }

        // GET: api/Books/author/5
        [HttpGet("author/{authorId}")]
        public async Task<ActionResult<IEnumerable<BookDto>>> GetBooksByAuthor(int authorId)
        {
            var books = await _bookService.GetBooksByAuthorIdAsync(authorId);
            return Ok(books);
        }

        // GET: api/Books/search?term=searchTerm
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<BookDto>>> SearchBooks([FromQuery] string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return BadRequest("Search term cannot be empty");
            }

            var books = await _bookService.SearchBooksAsync(term);
            return Ok(books);
        }

        // POST: api/Books
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<BookDto>> CreateBook(CreateBookDto bookDto)
        {
            try
            {
                var createdBook = await _bookService.CreateBookAsync(bookDto);
                return CreatedAtAction(nameof(GetBook), new { id = createdBook.Id }, createdBook);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Books/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateBook(int id, UpdateBookDto bookDto)
        {
            try
            {
                var updatedBook = await _bookService.UpdateBookAsync(id, bookDto);

                if (updatedBook == null)
                {
                    return NotFound();
                }

                return Ok(updatedBook);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/Books/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteBook(int id)
        {
            var result = await _bookService.DeleteBookAsync(id);

            if (!result)
            {
                return NotFound();
            }

            return NoContent();
        }
    }
} 