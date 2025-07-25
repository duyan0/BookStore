using AutoMapper;
using BookStore.Core.DTOs;
using BookStore.Core.Entities;
using BookStore.Core.Interfaces;
using BookStore.Core.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BookStore.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;
        private readonly IEmailService _emailService;

        public AuthService(
            IUserRepository userRepository,
            IMapper mapper,
            IConfiguration configuration,
            IEmailService emailService)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _configuration = configuration;
            _emailService = emailService;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginUserDto loginDto)
        {
            try
            {
                // Try to find user by username first, then by email
                User? user = null;

                // Check if the input looks like an email
                if (loginDto.Username.Contains("@"))
                {
                    user = await _userRepository.GetByEmailAsync(loginDto.Username);
                }
                else
                {
                    user = await _userRepository.GetByUsernameAsync(loginDto.Username);
                }

                // If not found by the primary method, try the other method
                if (user == null)
                {
                    if (loginDto.Username.Contains("@"))
                    {
                        user = await _userRepository.GetByUsernameAsync(loginDto.Username);
                    }
                    else
                    {
                        user = await _userRepository.GetByEmailAsync(loginDto.Username);
                    }
                }

                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email/Tên đăng nhập hoặc mật khẩu không đúng"
                    };
                }

                if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Email/Tên đăng nhập hoặc mật khẩu không đúng"
                    };
                }

                var token = GenerateJwtToken(user);
                var userDto = _mapper.Map<UserDto>(user);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Đăng nhập thành công",
                    Token = token,
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = $"Đăng nhập thất bại: {ex.Message}"
                };
            }
        }

        public async Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerDto)
        {
            // Check if username already exists
            var existingUserByUsername = await _userRepository.GetByUsernameAsync(registerDto.Username);
            if (existingUserByUsername != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Username already exists"
                };
            }

            // Check if email already exists
            var existingUserByEmail = await _userRepository.GetByEmailAsync(registerDto.Email);
            if (existingUserByEmail != null)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = "Email already exists"
                };
            }

            // Create new user
            var user = _mapper.Map<User>(registerDto);
            user.PasswordHash = HashPassword(registerDto.Password);

            var createdUser = await _userRepository.AddAsync(user);
            var token = GenerateJwtToken(createdUser);
            var userDto = _mapper.Map<UserDto>(createdUser);

            return new AuthResponseDto
            {
                Success = true,
                Message = "Registration successful",
                Token = token,
                User = userDto
            };
        }

        public async Task<UserDto?> GetUserByIdAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            return user != null ? _mapper.Map<UserDto>(user) : null;
        }

        public async Task<IEnumerable<UserDto>> GetAllUsersAsync()
        {
            var users = await _userRepository.GetAllAsync();
            return _mapper.Map<IEnumerable<UserDto>>(users);
        }

        public async Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return null;
                }

                // Update user properties
                user.FirstName = updateDto.FirstName;
                user.LastName = updateDto.LastName;
                user.Phone = updateDto.Phone ?? string.Empty;
                user.Address = updateDto.Address ?? string.Empty;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                return _mapper.Map<UserDto>(user);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> DeleteUserAsync(int id)
        {
            var user = await _userRepository.GetByIdAsync(id);
            if (user == null)
            {
                return false;
            }

            await _userRepository.DeleteAsync(user);
            return true;
        }

        public async Task<UserDto?> ToggleAdminStatusAsync(int id)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return null;
                }

                user.IsAdmin = !user.IsAdmin;
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                return _mapper.Map<UserDto>(user);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async Task<bool> ResetUserPasswordAsync(int id, string newPassword)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(id);
                if (user == null)
                {
                    return false;
                }

                user.PasswordHash = HashPassword(newPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<UserStatisticsDto> GetUserStatisticsAsync()
        {
            try
            {
                var users = await _userRepository.GetAllAsync();
                var usersList = users.ToList();

                var now = DateTime.UtcNow;
                var startOfMonth = new DateTime(now.Year, now.Month, 1);
                var startOfWeek = now.AddDays(-(int)now.DayOfWeek);
                var startOfDay = now.Date;

                return new UserStatisticsDto
                {
                    TotalUsers = usersList.Count,
                    AdminUsers = usersList.Count(u => u.IsAdmin),
                    RegularUsers = usersList.Count(u => !u.IsAdmin),
                    NewUsersThisMonth = usersList.Count(u => u.CreatedAt >= startOfMonth),
                    NewUsersThisWeek = usersList.Count(u => u.CreatedAt >= startOfWeek),
                    ActiveUsersToday = usersList.Count(u => u.UpdatedAt?.Date == startOfDay ||
                                                           (u.UpdatedAt == null && u.CreatedAt.Date == startOfDay)),
                    LastUpdated = DateTime.UtcNow
                };
            }
            catch (Exception)
            {
                return new UserStatisticsDto();
            }
        }

        // Test method để debug password hashing
        public bool TestPasswordHashing(string password, string storedHash)
        {
            return VerifyPasswordHash(password, storedHash);
        }

        public string TestHashPassword(string password)
        {
            return HashPassword(password);
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

        private string GenerateJwtToken(User user)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(
                _configuration["Jwt:Key"] ?? "DefaultSecretKeyWithAtLeast32Characters!"));
            
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.Email, user.Email)
            };

            if (user.IsAdmin)
            {
                claims.Add(new Claim(ClaimTypes.Role, "Admin"));
            }

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(claims),
                Expires = DateTime.UtcNow.AddDays(7),
                SigningCredentials = creds,
                Issuer = _configuration["Jwt:Issuer"],
                Audience = _configuration["Jwt:Audience"]
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var token = tokenHandler.CreateToken(tokenDescriptor);

            return tokenHandler.WriteToken(token);
        }

        public async Task<bool> ResetPasswordAsync(string email)
        {
            try
            {
                var user = await _userRepository.GetUserByEmailAsync(email);
                if (user == null)
                {
                    return false; // User not found
                }

                // Generate new random password
                var newPassword = GenerateRandomPassword();

                // Update user password
                user.PasswordHash = HashPassword(newPassword);
                await _userRepository.UpdateAsync(user);

                // Send email with new password
                var emailSent = await _emailService.SendPasswordResetEmailAsync(user.Email, newPassword, user.Username);

                return emailSent;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ResetPasswordByUsernameAsync(string username)
        {
            try
            {
                var user = await _userRepository.GetUserByUsernameAsync(username);
                if (user == null)
                {
                    return false; // User not found
                }

                // Generate new random password
                var newPassword = GenerateRandomPassword();

                // Update user password
                user.PasswordHash = HashPassword(newPassword);
                await _userRepository.UpdateAsync(user);

                // Send email with new password
                var emailSent = await _emailService.SendPasswordResetEmailAsync(user.Email, newPassword, user.Username);

                return emailSent;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private string GenerateRandomPassword(int length = 8)
        {
            const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghijkmnpqrstuvwxyz23456789";
            var random = new Random();
            var password = new char[length];

            for (int i = 0; i < length; i++)
            {
                password[i] = chars[random.Next(chars.Length)];
            }

            return new string(password);
        }

        public async Task<bool> UpdateUserAvatarAsync(int userId, string avatarUrl)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                user.AvatarUrl = avatarUrl;
                await _userRepository.UpdateAsync(user);
                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }

        public async Task<bool> ChangePasswordAsync(int userId, ChangePasswordDto changePasswordDto)
        {
            try
            {
                var user = await _userRepository.GetByIdAsync(userId);
                if (user == null)
                {
                    return false;
                }

                // Verify current password
                if (!VerifyPasswordHash(changePasswordDto.CurrentPassword, user.PasswordHash))
                {
                    return false;
                }

                // Hash new password
                user.PasswordHash = HashPassword(changePasswordDto.NewPassword);
                user.UpdatedAt = DateTime.UtcNow;

                await _userRepository.UpdateAsync(user);

                // Send email notification about password change
                try
                {
                    await _emailService.SendPasswordChangeNotificationAsync(user.Email, user.FirstName);
                }
                catch (Exception)
                {
                    // Log email error but don't fail the password change
                    // Password change succeeded, email notification failed
                }

                return true;
            }
            catch (Exception)
            {
                return false;
            }
        }
    }
}