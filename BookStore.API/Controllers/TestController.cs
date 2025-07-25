using BookStore.Core.Entities;
using BookStore.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BookStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public TestController(ApplicationDbContext context)
        {
            _context = context;
        }

        // GET: api/Test/SeedAdminForce
        [HttpGet("SeedAdminForce")]
        public async Task<ActionResult<string>> SeedAdminForce()
        {
            try
            {
                // Xóa admin user cũ nếu có
                var existingAdmin = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == "admin");
                
                if (existingAdmin != null)
                {
                    _context.Users.Remove(existingAdmin);
                    await _context.SaveChangesAsync();
                }

                // Tạo admin user mới
                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@bookstore.com",
                    PasswordHash = HashPassword("Admin@123"),
                    FirstName = "Admin",
                    LastName = "User",
                    Phone = "1234567890",
                    Address = "Admin Address",
                    IsAdmin = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Users.AddAsync(adminUser);
                await _context.SaveChangesAsync();

                return Ok("Admin user created successfully with force");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Test/SeedAdmin
        [HttpGet("SeedAdmin")]
        public async Task<ActionResult<string>> SeedAdmin()
        {
            try
            {
                // Check if admin user already exists
                var adminExists = await _context.Users.AnyAsync(u => u.Username == "admin");
                if (adminExists)
                {
                    return Ok("Admin user already exists");
                }

                // Seed Admin User
                var adminUser = new User
                {
                    Username = "admin",
                    Email = "admin@bookstore.com",
                    PasswordHash = HashPassword("Admin@123"),
                    FirstName = "Admin",
                    LastName = "User",
                    Phone = "1234567890",
                    Address = "Admin Address",
                    IsAdmin = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Users.AddAsync(adminUser);
                await _context.SaveChangesAsync();

                return Ok("Admin user created successfully");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Test/Users
        [HttpGet("Users")]
        public async Task<ActionResult<IEnumerable<User>>> GetUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Test/AllUsers
        [HttpGet("AllUsers")]
        public async Task<ActionResult<IEnumerable<User>>> GetAllUsers()
        {
            try
            {
                var users = await _context.Users.ToListAsync();
                return Ok(users);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Test/TestLogin
        [HttpPost("TestLogin")]
        public async Task<ActionResult<string>> TestLogin([FromBody] LoginDto model)
        {
            try
            {
                var user = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username.ToLower() == model.Username.ToLower());
                
                if (user == null)
                {
                    return BadRequest("User not found");
                }

                bool isPasswordValid = VerifyPasswordHash(model.Password, user.PasswordHash);
                
                if (!isPasswordValid)
                {
                    return BadRequest("Invalid password");
                }

                return Ok($"Login successful for user: {user.Username}, IsAdmin: {user.IsAdmin}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Test/CreateSimpleAdmin
        [HttpGet("CreateSimpleAdmin")]
        public async Task<ActionResult<string>> CreateSimpleAdmin()
        {
            try
            {
                // Xóa admin user cũ nếu có
                var existingAdmin = await _context.Users
                    .FirstOrDefaultAsync(u => u.Username == "admintest");
                
                if (existingAdmin != null)
                {
                    _context.Users.Remove(existingAdmin);
                    await _context.SaveChangesAsync();
                }

                // Tạo admin user mới với mật khẩu đơn giản (không hash)
                var adminUser = new User
                {
                    Username = "admintest",
                    Email = "admintest@bookstore.com",
                    PasswordHash = "Admin@123", // Mật khẩu đơn giản không hash
                    FirstName = "Admin",
                    LastName = "Test",
                    Phone = "1234567890",
                    Address = "Admin Address",
                    IsAdmin = true,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Users.AddAsync(adminUser);
                await _context.SaveChangesAsync();

                return Ok($"Simple admin user created: {adminUser.Username}, Password: Admin@123");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Test/DecodeToken
        [HttpGet("DecodeToken")]
        public ActionResult<object> DecodeToken([FromQuery] string token)
        {
            try
            {
                if (string.IsNullOrEmpty(token))
                {
                    return BadRequest("Token is required");
                }
                
                // Loại bỏ Bearer nếu có
                if (token.StartsWith("Bearer ", StringComparison.OrdinalIgnoreCase))
                {
                    token = token.Substring(7);
                }
                
                // Parse JWT token mà không xác thực
                var handler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var jwtToken = handler.ReadJwtToken(token);
                
                // Lấy claims
                var claims = jwtToken.Claims.Select(c => new { c.Type, c.Value }).ToList();
                
                return Ok(new
                {
                    token_info = new
                    {
                        jwtToken.ValidTo,
                        jwtToken.ValidFrom,
                        jwtToken.Issuer,
                        jwtToken.Audiences
                    },
                    claims,
                    help = "Đảm bảo role claim có giá trị 'Admin' và đã thêm 'Bearer ' phía trước token"
                });
            }
            catch (Exception ex)
            {
                return BadRequest($"Invalid token: {ex.Message}");
            }
        }

        // GET: api/Test/GenerateToken
        [HttpGet("GenerateToken")]
        public ActionResult<string> GenerateToken()
        {
            try
            {
                var key = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(
                    System.Text.Encoding.UTF8.GetBytes("ThisIsASecretKeyForJwtAuthenticationInBookStoreAPI_VeryVeryLongKeyToSolveIssueWithHmacSha512"));

                var creds = new Microsoft.IdentityModel.Tokens.SigningCredentials(key,
                    Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256);

                var claims = new List<System.Security.Claims.Claim>
                {
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.NameIdentifier, "2"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Name, "admintest"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Email, "admintest@bookstore.com"),
                    new System.Security.Claims.Claim(System.Security.Claims.ClaimTypes.Role, "Admin")
                };

                var tokenDescriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor
                {
                    Subject = new System.Security.Claims.ClaimsIdentity(claims),
                    Expires = DateTime.UtcNow.AddDays(7),
                    SigningCredentials = creds,
                    Issuer = "BookStoreAPI",
                    Audience = "BookStoreClient"
                };

                var tokenHandler = new System.IdentityModel.Tokens.Jwt.JwtSecurityTokenHandler();
                var token = tokenHandler.CreateToken(tokenDescriptor);
                var tokenString = tokenHandler.WriteToken(token);

                return Ok(tokenString);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Test/TestPasswordHashing
        [HttpPost("TestPasswordHashing")]
        public ActionResult<object> TestPasswordHashing([FromBody] TestPasswordRequest request)
        {
            try
            {
                var hashedPassword = HashPassword(request.Password);
                var isValid = VerifyPasswordHash(request.Password, hashedPassword);

                // Test với stored hash nếu có
                bool isStoredHashValid = false;
                if (!string.IsNullOrEmpty(request.StoredHash))
                {
                    isStoredHashValid = VerifyPasswordHash(request.Password, request.StoredHash);
                }

                return Ok(new
                {
                    OriginalPassword = request.Password,
                    NewHashedPassword = hashedPassword,
                    IsNewHashValid = isValid,
                    StoredHash = request.StoredHash,
                    IsStoredHashValid = isStoredHashValid,
                    Message = $"New hash valid: {isValid}, Stored hash valid: {isStoredHashValid}"
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    Error = ex.Message,
                    Message = "Password testing failed"
                });
            }
        }

        private string HashPassword(string password)
        {
            // Store password as plain text (as requested)
            return password;
        }

        private bool VerifyPasswordHash(string password, string storedPassword)
        {
            // Simple plain text comparison (as requested)
            return password == storedPassword;
        }

        public class LoginDto
        {
            public string Username { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
        }

        public class TestPasswordRequest
        {
            public string Password { get; set; } = string.Empty;
            public string? StoredHash { get; set; }
        }
    }
} 