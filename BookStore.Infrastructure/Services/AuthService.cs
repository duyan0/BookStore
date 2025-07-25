using AutoMapper;
using BookStore.Core.DTOs;
using BookStore.Core.Entities;
using BookStore.Core.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace BookStore.Infrastructure.Services
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IMapper _mapper;
        private readonly IConfiguration _configuration;

        public AuthService(
            IUserRepository userRepository,
            IMapper mapper,
            IConfiguration configuration)
        {
            _userRepository = userRepository;
            _mapper = mapper;
            _configuration = configuration;
        }

        public async Task<AuthResponseDto> LoginAsync(LoginUserDto loginDto)
        {
            try
            {
                var user = await _userRepository.GetByUsernameAsync(loginDto.Username);
                if (user == null)
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    };
                }

                if (!VerifyPasswordHash(loginDto.Password, user.PasswordHash))
                {
                    return new AuthResponseDto
                    {
                        Success = false,
                        Message = "Invalid username or password"
                    };
                }

                var token = GenerateJwtToken(user);
                var userDto = _mapper.Map<UserDto>(user);

                return new AuthResponseDto
                {
                    Success = true,
                    Message = "Login successful",
                    Token = token,
                    User = userDto
                };
            }
            catch (Exception ex)
            {
                return new AuthResponseDto
                {
                    Success = false,
                    Message = $"Login failed: {ex.Message}"
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
            using var hmac = new HMACSHA512();
            var salt = hmac.Key;
            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

            // Combine salt and hash
            var hashBytes = new byte[salt.Length + hash.Length];
            Array.Copy(salt, 0, hashBytes, 0, salt.Length);
            Array.Copy(hash, 0, hashBytes, salt.Length, hash.Length);

            return Convert.ToBase64String(hashBytes);
        }

        private bool VerifyPasswordHash(string password, string storedHash)
        {
            try
            {
                // Decode the stored hash
                var hashBytes = Convert.FromBase64String(storedHash);

                // Check if the hash has the expected length (64 bytes salt + 64 bytes hash)
                if (hashBytes.Length != 128)
                {
                    return false;
                }

                // Extract salt (first 64 bytes)
                var salt = new byte[64];
                Array.Copy(hashBytes, 0, salt, 0, 64);

                // Create hmac with the same salt
                using var hmac = new HMACSHA512(salt);
                var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(password));

                // Compare computed hash with stored hash (last 64 bytes)
                for (int i = 0; i < computedHash.Length; i++)
                {
                    if (computedHash[i] != hashBytes[64 + i])
                    {
                        return false;
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                // Log the error for debugging
                Console.WriteLine($"Password verification error: {ex.Message}");
                return false;
            }
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
    }
} 