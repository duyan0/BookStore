using BookStore.Core.DTOs;

namespace BookStore.Core.Interfaces
{
    public interface IAuthService
    {
        Task<AuthResponseDto> RegisterAsync(RegisterUserDto registerDto);
        Task<AuthResponseDto> LoginAsync(LoginUserDto loginDto);
        Task<UserDto?> GetUserByIdAsync(int id);
        Task<IEnumerable<UserDto>> GetAllUsersAsync();
        Task<UserDto?> UpdateUserAsync(int id, UpdateUserDto updateDto);
        Task<bool> DeleteUserAsync(int id);
        Task<UserDto?> ToggleAdminStatusAsync(int id);
        Task<bool> ResetUserPasswordAsync(int id, string newPassword);
        Task<UserStatisticsDto> GetUserStatisticsAsync();
    }
} 