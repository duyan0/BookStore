using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BookStore.Web.Services;
using BookStore.Web.Models;
using BookStore.Core.DTOs;

namespace BookStore.Web.Controllers
{
    public class VouchersController : Controller
    {
        private readonly ApiService _apiService;
        private readonly ILogger<VouchersController> _logger;

        public VouchersController(ApiService apiService, ILogger<VouchersController> logger)
        {
            _apiService = apiService;
            _logger = logger;
        }

        // GET: Vouchers/Available
        public async Task<IActionResult> Available()
        {
            try
            {
                // Check if user is authenticated via session
                if (!IsUserLoggedIn())
                {
                    return RedirectToAction("Login", "Account");
                }
                var vouchers = await _apiService.GetAsync<List<VoucherDto>>("vouchers/available");
                
                var viewModel = new VouchersViewModel
                {
                    AvailableVouchers = vouchers ?? new List<VoucherDto>(),
                    PageTitle = "Voucher có thể sử dụng",
                    ShowAvailable = true
                };

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving available vouchers");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách voucher.";
                return View("Index", new VouchersViewModel());
            }
        }

        // GET: Vouchers/MyVouchers
        public async Task<IActionResult> MyVouchers()
        {
            try
            {
                // Check if user is authenticated via session
                if (!IsUserLoggedIn())
                {
                    return RedirectToAction("Login", "Account");
                }
                var userId = GetCurrentUserId();
                var vouchers = await _apiService.GetAsync<List<VoucherDto>>($"vouchers/user/{userId}");
                
                var viewModel = new VouchersViewModel
                {
                    MyVouchers = vouchers ?? new List<VoucherDto>(),
                    PageTitle = "Voucher của tôi",
                    ShowMyVouchers = true
                };

                return View("Index", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user vouchers");
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải danh sách voucher của bạn.";
                return View("Index", new VouchersViewModel());
            }
        }

        // GET: Vouchers/Details/{id}
        public async Task<IActionResult> Details(int id)
        {
            try
            {
                var voucher = await _apiService.GetAsync<VoucherDto>($"vouchers/{id}");
                
                if (voucher == null)
                {
                    TempData["ErrorMessage"] = "Không tìm thấy voucher.";
                    return RedirectToAction(nameof(Available));
                }

                return View(voucher);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving voucher details for ID: {VoucherId}", id);
                TempData["ErrorMessage"] = "Có lỗi xảy ra khi tải thông tin voucher.";
                return RedirectToAction(nameof(Available));
            }
        }

        // POST: Vouchers/ValidateCode
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ValidateCode(string voucherCode, decimal orderAmount = 0)
        {
            try
            {
                if (string.IsNullOrEmpty(voucherCode))
                {
                    return Json(new { success = false, message = "Vui lòng nhập mã voucher." });
                }

                var userId = GetCurrentUserId();
                var validationDto = new
                {
                    Code = voucherCode,
                    OrderAmount = orderAmount,
                    UserId = userId
                };

                var result = await _apiService.PostAsync<dynamic>("vouchers/validate", validationDto);
                
                if (result != null)
                {
                    return Json(new { 
                        success = result.isValid, 
                        message = result.message,
                        discountAmount = result.discountAmount,
                        freeShipping = result.freeShipping,
                        voucherName = result.voucherName,
                        voucherType = result.voucherType
                    });
                }
                else
                {
                    return Json(new { success = false, message = "Không thể kiểm tra voucher. Vui lòng thử lại." });
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error validating voucher code: {VoucherCode}", voucherCode);
                return Json(new { success = false, message = "Có lỗi xảy ra khi kiểm tra voucher." });
            }
        }

        // Helper methods
        private int GetCurrentUserId()
        {
            var userId = HttpContext.Session.GetInt32("UserId");
            if (userId.HasValue)
            {
                return userId.Value;
            }
            throw new UnauthorizedAccessException("User not found");
        }

        private bool IsUserLoggedIn()
        {
            var token = HttpContext.Session.GetString("Token");
            var userId = HttpContext.Session.GetInt32("UserId");
            return !string.IsNullOrEmpty(token) && userId.HasValue;
        }
    }
}
