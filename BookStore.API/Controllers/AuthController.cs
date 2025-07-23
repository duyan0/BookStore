using BookStore.Core.DTOs;
using BookStore.Core.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BookStore.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        // POST: api/Auth/register
        [HttpPost("register")]
        public async Task<ActionResult<AuthResponseDto>> Register(RegisterUserDto registerDto)
        {
            var response = await _authService.RegisterAsync(registerDto);

            if (!response.Success)
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        // POST: api/Auth/login
        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginUserDto loginDto)
        {
            var response = await _authService.LoginAsync(loginDto);

            if (!response.Success)
            {
                return Unauthorized(response);
            }

            return Ok(response);
        }

        // GET: api/Auth/users
        [HttpGet("users")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<IEnumerable<UserDto>>> GetUsers()
        {
            try
            {
                // Kiểm tra xác thực
                var identity = User.Identity;
                var isAuthenticated = identity?.IsAuthenticated ?? false;
                var name = identity?.Name;
                var roles = User.Claims.Where(c => c.Type == System.Security.Claims.ClaimTypes.Role).Select(c => c.Value).ToList();
                
                if (!isAuthenticated)
                {
                    return Unauthorized("User is not authenticated");
                }
                
                if (!User.IsInRole("Admin"))
                {
                    return Forbid("User is not in Admin role");
                }
                
                var users = await _authService.GetAllUsersAsync();
                return Ok(new 
                { 
                    users,
                    authInfo = new
                    {
                        isAuthenticated,
                        name,
                        roles
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Auth/users/5
        [HttpGet("users/{id}")]
        [Authorize]
        public async Task<ActionResult<UserDto>> GetUser(int id)
        {
            var user = await _authService.GetUserByIdAsync(id);

            if (user == null)
            {
                return NotFound();
            }

            // Check if the user is requesting their own data or is an admin
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var isAdmin = User.IsInRole("Admin");

            if (!isAdmin && userId != id.ToString())
            {
                return Forbid();
            }

            return Ok(user);
        }

        // PUT: api/Auth/users/5
        [HttpPut("users/{id}")]
        [Authorize]
        public async Task<ActionResult<UserDto>> UpdateUser(int id, UpdateUserDto updateDto)
        {
            try
            {
                // Check if the user is updating their own data or is an admin
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                var isAdmin = User.IsInRole("Admin");

                if (!isAdmin && userId != id.ToString())
                {
                    return Forbid();
                }

                var updatedUser = await _authService.UpdateUserAsync(id, updateDto);

                if (updatedUser == null)
                {
                    return NotFound();
                }

                return Ok(updatedUser);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // DELETE: api/Auth/users/5
        [HttpDelete("users/{id}")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> DeleteUser(int id)
        {
            try
            {
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // Prevent admin from deleting themselves
                if (currentUserId == id.ToString())
                {
                    return BadRequest(new { message = "Cannot delete your own account" });
                }

                var result = await _authService.DeleteUserAsync(id);

                if (!result)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { message = "User deleted successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // PUT: api/Auth/users/5/toggle-admin
        [HttpPut("users/{id}/toggle-admin")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult<UserDto>> ToggleAdminStatus(int id)
        {
            try
            {
                var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;

                // Prevent admin from removing their own admin status
                if (currentUserId == id.ToString())
                {
                    return BadRequest(new { message = "Cannot modify your own admin status" });
                }

                var result = await _authService.ToggleAdminStatusAsync(id);

                if (result == null)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // POST: api/Auth/users/5/reset-password
        [HttpPost("users/{id}/reset-password")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> ResetUserPassword(int id, [FromBody] ResetPasswordDto resetDto)
        {
            try
            {
                var result = await _authService.ResetUserPasswordAsync(id, resetDto.NewPassword);

                if (!result)
                {
                    return NotFound(new { message = "User not found" });
                }

                return Ok(new { message = "Password reset successfully" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        // GET: api/Auth/users/statistics
        [HttpGet("users/statistics")]
        [Authorize(Roles = "Admin")]
        public async Task<ActionResult> GetUserStatistics()
        {
            try
            {
                var stats = await _authService.GetUserStatisticsAsync();
                return Ok(stats);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }


    }
} 