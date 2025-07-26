using Microsoft.AspNetCore.Mvc;
using BookStore.Web.Services;
using BookStore.Web.Models;
using BookStore.Core.DTOs;
using BookStore.Web.Attributes;

namespace BookStore.Web.Areas.Admin.Controllers
{
    [Area("Admin")]
    [AdminOnly]
    public class VouchersController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<VouchersController> _logger;

        public VouchersController(ApiService apiService, ILogger<VouchersController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Admin/Vouchers
        public async Task<IActionResult> Index()
        {
            try
            {
                var vouchers = await _apiService.GetAsync<List<VoucherDto>>("vouchers");
                var viewModels = vouchers?.Select(MapToViewModel).ToList() ?? new List<VoucherViewModel>();
                return View(viewModels);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading vouchers");
                TempData["Error"] = "Không thể tải danh sách voucher.";
                return View(new List<VoucherViewModel>());
            }
        }

        // GET: Admin/Vouchers/Details/5
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var voucher = await _apiService.GetAsync<VoucherDto>($"vouchers/{id}");
                if (voucher == null)
                {
                    TempData["Error"] = "Không tìm thấy voucher.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = MapToViewModel(voucher);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading voucher details for ID: {VoucherId}", id);
                TempData["Error"] = "Không thể tải thông tin voucher.";
                return RedirectToAction(nameof(Index));
            }
        }

        // GET: Admin/Vouchers/Create
        public IActionResult Create()
        {
            var model = new CreateVoucherViewModel
            {
                StartDate = DateTime.Now,
                EndDate = DateTime.Now.AddDays(30),
                IsActive = true
            };
            return View(model);
        }

        // POST: Admin/Vouchers/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(CreateVoucherViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            try
            {
                var createDto = MapToCreateDto(model);
                _logger.LogInformation("Creating voucher with data: {@CreateDto}", createDto);

                var createdVoucher = await _apiService.PostAsync<VoucherDto>("vouchers", createDto);

                if (createdVoucher != null)
                {
                    _logger.LogInformation("Voucher created successfully with ID: {VoucherId}", createdVoucher.Id);
                    TempData["Success"] = "Tạo voucher thành công!";
                    return RedirectToAction(nameof(Details), new { id = createdVoucher.Id });
                }
                else
                {
                    _logger.LogWarning("API returned null when creating voucher");
                    TempData["Error"] = "Không thể tạo voucher. Vui lòng thử lại.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating voucher with model: {@Model}", model);
                TempData["Error"] = $"Có lỗi xảy ra khi tạo voucher: {ex.Message}";
            }

            return View(model);
        }

        // GET: Admin/Vouchers/Edit/5
        public async Task<IActionResult> Edit(int id)
        {
            try
            {
                var voucher = await _apiService.GetAsync<VoucherDto>($"vouchers/{id}");
                if (voucher == null)
                {
                    TempData["Error"] = "Không tìm thấy voucher.";
                    return RedirectToAction(nameof(Index));
                }

                var viewModel = MapToEditViewModel(voucher);
                return View(viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error loading voucher for edit, ID: {VoucherId}", id);
                TempData["Error"] = "Không thể tải thông tin voucher.";
                return RedirectToAction(nameof(Index));
            }
        }

        // POST: Admin/Vouchers/Edit/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, EditVoucherViewModel model)
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
                var updateDto = MapToUpdateDto(model);
                var updatedVoucher = await _apiService.PutAsync<VoucherDto>($"vouchers/{id}", updateDto);

                if (updatedVoucher != null)
                {
                    TempData["Success"] = "Cập nhật voucher thành công!";
                    return RedirectToAction(nameof(Details), new { id });
                }
                else
                {
                    TempData["Error"] = "Không thể cập nhật voucher. Vui lòng thử lại.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating voucher ID: {VoucherId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi cập nhật voucher.";
            }

            return View(model);
        }

        // POST: Admin/Vouchers/Delete/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var success = await _apiService.DeleteAsync($"vouchers/{id}");
                if (success)
                {
                    TempData["Success"] = "Xóa voucher thành công!";
                }
                else
                {
                    TempData["Error"] = "Không thể xóa voucher.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting voucher ID: {VoucherId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi xóa voucher.";
            }

            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Vouchers/Activate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Activate(int id)
        {
            try
            {
                var success = await _apiService.PostAsync<bool>($"vouchers/{id}/activate", new { });
                if (success)
                {
                    TempData["Success"] = "Kích hoạt voucher thành công!";
                }
                else
                {
                    TempData["Error"] = "Không thể kích hoạt voucher.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error activating voucher ID: {VoucherId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi kích hoạt voucher.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        // POST: Admin/Vouchers/Deactivate/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Deactivate(int id)
        {
            try
            {
                var success = await _apiService.PostAsync<bool>($"vouchers/{id}/deactivate", new { });
                if (success)
                {
                    TempData["Success"] = "Vô hiệu hóa voucher thành công!";
                }
                else
                {
                    TempData["Error"] = "Không thể vô hiệu hóa voucher.";
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deactivating voucher ID: {VoucherId}", id);
                TempData["Error"] = "Có lỗi xảy ra khi vô hiệu hóa voucher.";
            }

            return RedirectToAction(nameof(Details), new { id });
        }

        #region Helper Methods

        private static VoucherViewModel MapToViewModel(VoucherDto dto)
        {
            return new VoucherViewModel
            {
                Id = dto.Id,
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                Value = dto.Value,
                MinimumOrderAmount = dto.MinimumOrderAmount,
                MaximumDiscountAmount = dto.MaximumDiscountAmount,
                UsageLimit = dto.UsageLimit,
                UsedCount = dto.UsedCount,
                UsageLimitPerUser = dto.UsageLimitPerUser,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive,
                CreatedAt = dto.CreatedAt,
                UpdatedAt = dto.UpdatedAt
            };
        }

        private static CreateVoucherDto MapToCreateDto(CreateVoucherViewModel model)
        {
            return new CreateVoucherDto
            {
                Code = model.Code.ToUpper(),
                Name = model.Name,
                Description = model.Description,
                Type = model.Type,
                Value = model.Value,
                MinimumOrderAmount = model.MinimumOrderAmount,
                MaximumDiscountAmount = model.MaximumDiscountAmount,
                UsageLimit = model.UsageLimit,
                UsageLimitPerUser = model.UsageLimitPerUser,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                IsActive = model.IsActive
            };
        }

        private static EditVoucherViewModel MapToEditViewModel(VoucherDto dto)
        {
            return new EditVoucherViewModel
            {
                Id = dto.Id,
                Code = dto.Code,
                Name = dto.Name,
                Description = dto.Description,
                Type = dto.Type,
                Value = dto.Value,
                MinimumOrderAmount = dto.MinimumOrderAmount,
                MaximumDiscountAmount = dto.MaximumDiscountAmount,
                UsageLimit = dto.UsageLimit,
                UsageLimitPerUser = dto.UsageLimitPerUser,
                StartDate = dto.StartDate,
                EndDate = dto.EndDate,
                IsActive = dto.IsActive,
                UsedCount = dto.UsedCount,
                CreatedAt = dto.CreatedAt
            };
        }

        private static UpdateVoucherDto MapToUpdateDto(EditVoucherViewModel model)
        {
            return new UpdateVoucherDto
            {
                Code = model.Code,
                Name = model.Name,
                Description = model.Description,
                Type = model.Type,
                Value = model.Value,
                MinimumOrderAmount = model.MinimumOrderAmount,
                MaximumDiscountAmount = model.MaximumDiscountAmount,
                UsageLimit = model.UsageLimit,
                UsageLimitPerUser = model.UsageLimitPerUser,
                StartDate = model.StartDate,
                EndDate = model.EndDate,
                IsActive = model.IsActive
            };
        }

        #endregion
    }
}
