using BookStore.Core.DTOs;
using BookStore.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthorsController : ControllerBase
    {
        private readonly IAuthorService _authorService;

        public AuthorsController(IAuthorService authorService)
        {
            _authorService = authorService;
        }

        // GET: api/Authors
        [HttpGet]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> GetAuthors()
        {
            var authors = await _authorService.GetAllAuthorsAsync();
            return Ok(authors);
        }

        // GET: api/Authors/5
        [HttpGet("{id}")]
        public async Task<ActionResult<AuthorDto>> GetAuthor(int id)
        {
            var author = await _authorService.GetAuthorByIdAsync(id);

            if (author == null)
            {
                return NotFound();
            }

            return Ok(author);
        }

        // GET: api/Authors/search?term=searchTerm
        [HttpGet("search")]
        public async Task<ActionResult<IEnumerable<AuthorDto>>> SearchAuthors([FromQuery] string term)
        {
            if (string.IsNullOrEmpty(term))
            {
                return BadRequest("Search term cannot be empty");
            }

            var authors = await _authorService.SearchAuthorsAsync(term);
            return Ok(authors);
        }

        // POST: api/Authors
        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<AuthorDto>> CreateAuthor(CreateAuthorDto authorDto)
        {
            try
            {
                var createdAuthor = await _authorService.CreateAuthorAsync(authorDto);
                return CreatedAtAction(nameof(GetAuthor), new { id = createdAuthor.Id }, createdAuthor);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // PUT: api/Authors/5
        [HttpPut("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> UpdateAuthor(int id, UpdateAuthorDto authorDto)
        {
            try
            {
                var updatedAuthor = await _authorService.UpdateAuthorAsync(id, authorDto);

                if (updatedAuthor == null)
                {
                    return NotFound();
                }

                return Ok(updatedAuthor);
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // DELETE: api/Authors/5
        [HttpDelete("{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteAuthor(int id)
        {
            try
            {
                var result = await _authorService.DeleteAuthorAsync(id);

                if (!result)
                {
                    return NotFound();
                }

                return NoContent();
            }
            catch (Exception ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
} 