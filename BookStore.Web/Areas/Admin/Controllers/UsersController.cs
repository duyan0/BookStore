using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Services;
using BookStore.Core.DTOs;
using BookStore.Web.Attributes;
using BookStore.Web.Models;

namespace BookStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class UsersController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<UsersController> _logger;

        public UsersController(ApiService apiService, ILogger<UsersController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Admin/Users
        public async Task<IActionResult> Index(string? search, string? sortBy, string? sortOrder, int page = 1, int pageSize = 10)
        {
            try
            {
                // Get all users from API
                var response = await _apiService.GetAsync<dynamic>("auth/users");
                var users = new List<UserDto>();

                if (response != null && response.users != null)
                {
                    foreach (var userObj in response.users)
                    {
                        users.Add(new UserDto
                        {
                            Id = userObj.id,
                            Username = userObj.username?.ToString() ?? "",
                            Email = userObj.email?.ToString() ?? "",
                            FirstName = userObj.firstName?.ToString() ?? "",
                            LastName = userObj.lastName?.ToString() ?? "",
                            Phone = userObj.phone?.ToString() ?? "",
                            Address = userObj.address?.ToString() ?? "",
                            IsAdmin = userObj.isAdmin ?? false,
                            CreatedAt = userObj.createdAt ?? DateTime.MinValue,
                            UpdatedAt = userObj.updatedAt
                        });
                    }
                }

                // Apply search filter
                if (!string.IsNullOrEmpty(search))
                {
                    users = users.Where(u => 
                        u.Username.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        u.Email.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        u.FirstName.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                        u.LastName.Contains(search, StringComparison.OrdinalIgnoreCase)
                    ).ToList();
                }

                // Apply sorting
                users = sortBy?.ToLower() switch
                {
                    "username" => sortOrder == "desc" ? users.OrderByDescending(u => u.Username).ToList() : users.OrderBy(u => u.Username).ToList(),
                    "email" => sortOrder == "desc" ? users.OrderByDescending(u => u.Email).ToList() : users.OrderBy(u => u.Email).ToList(),
                    "fullname" => sortOrder == "desc" ? users.OrderByDescending(u => u.FullName).ToList() : users.OrderBy(u => u.FullName).ToList(),
                    "createdat" => sortOrder == "desc" ? users.OrderByDescending(u => u.CreatedAt).ToList() : users.OrderBy(u => u.CreatedAt).ToList(),
                    "isadmin" => sortOrder == "desc" ? users.OrderByDescending(u => u.IsAdmin).ToList() : users.OrderBy(u => u.IsAdmin).ToList(),
                    _ => users.OrderBy(u => u.Username).ToList()
                };

                // Apply pagination
                var totalUsers = users.Count;
                var totalPages = (int)Math.Ceiling((double)totalUsers / pageSize);
                var paginatedUsers = users.Skip((page - 1) * pageSize).Take(pageSize).ToList();

                var viewModel = new UserManagementViewModel
                {
                    Users = paginatedUsers,
                    CurrentPage = page,
                    TotalPages = totalPages,
                    PageSize = pageSize,
                    TotalUsers = totalUsers,
                    Search = search,
                    SortBy = sortBy,
                    SortOrder = sortOrder
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading users");
                TempData["Error"] = "Có lỗi xảy ra khi tải danh sách người dùng.";
                return View(new UserManagementViewModel());
            }
        }

        // GET: Admin/Users/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var user = await _apiService.GetAsync<UserDto>($"auth/users/{id}");
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng.";
                    return RedirectToAction(nameof(Index));
                }

                return View(user);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user details for ID: {UserId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin người dùng.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Users/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var user = await _apiService.GetAsync<UserDto>($"auth/users/{id}");
                if (user == null)
                {
                    TempData["Error"] = "Không tìm thấy người dùng.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = new EditUserViewModel
                {
                    Id = user.Id,
                    Username = user.Username,
                    Email = user.Email,
                    FirstName = user.FirstName,
                    LastName = user.LastName,
                    Phone = user.Phone,
                    Address = user.Address,
                    IsAdmin = user.IsAdmin
                };

                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user for edit, ID: {UserId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi tải thông tin người dùng.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Users/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditUserViewModel model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var updateDto = new UpdateUserDto
                {
                    FirstName = model.FirstName,
                    LastName = model.LastName,
                    Phone = model.Phone,
                    Address = model.Address
                };

                var result = await _apiService.PutAsync<UserDto>($"auth/users/{id}", updateDto);
                
                if (result != null)
                {
                    TempData["Success"] = "Cập nhật thông tin người dùng thành công.";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi cập nhật thông tin người dùng.";
                    return View(model);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating user ID: {UserId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật thông tin người dùng.";
                return View(model);
            }
        }

        // POST: Admin/Users/ToggleAdmin/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ToggleAdmin(int id)
        {
            try
            {
                var result = await _apiService.PutAsync<UserDto>($"auth/users/{id}/toggle-admin", new { });
                
                if (result != null)
                {
                    var action = result.IsAdmin ? "thăng cấp thành Admin" : "hạ cấp xuống User thường";
                    TempData["Success"] = $"Đã {action} thành công.";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi thay đổi quyền người dùng.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error toggling admin status for user ID: {UserId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi thay đổi quyền người dùng.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Admin/Users/ResetPassword/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(int id, string newPassword)
        {
            if (string.IsNullOrEmpty(newPassword) || newPassword.Length < 6)
            {
                TempData["Error"] = "Mật khẩu mới phải có ít nhất 6 ký tự.";
                return RedirectToAction(nameof(Details), new { id });
            }

            try
            {
                var resetDto = new ResetPasswordDto { NewPassword = newPassword };
                var response = await _apiService.PostAsync<dynamic>($"auth/users/{id}/reset-password", resetDto);
                
                if (response != null)
                {
                    TempData["Success"] = "Đặt lại mật khẩu thành công.";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi đặt lại mật khẩu.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error resetting password for user ID: {UserId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi đặt lại mật khẩu.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Admin/Users/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var response = await _apiService.DeleteAsync($"auth/users/{id}");
                
                if (response)
                {
                    TempData["Success"] = "Xóa người dùng thành công.";
                }
                else
                {
                    TempData["Error"] = "Có lỗi xảy ra khi xóa người dùng.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting user ID: {UserId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa người dùng.";
            }

            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Users/Statistics
        public async Task<IActionResult> Statistics()
        {
            try
            {
                var stats = await _apiService.GetAsync<UserStatisticsDto>("auth/users/statistics");
                return View(stats ?? new UserStatisticsDto());
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading user statistics");
                TempData["Error"] = "Có lỗi xảy ra khi tải thống kê người dùng.";
                return View(new UserStatisticsDto());
            }
        }
    }
}
