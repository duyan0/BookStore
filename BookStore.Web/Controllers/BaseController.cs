using Microsoft.AspNetCore.Mvc;

namespace BookStore.Web.Controllers
{
    public class BaseController : Controller
    {
        protected IActionResult HandleUnauthorized()
        {
            TempData["Warning"] = "Phiên đăng nhập đã hết hạn. Vui lòng đăng nhập lại.";
            return RedirectToAction("Login", "Account");
        }

        protected IActionResult HandleAccessDenied()
        {
            return View("AccessDenied");
        }

        protected IActionResult HandleApiError(Exception ex, string defaultMessage)
        {
            if (ex is UnauthorizedAccessException)
            {
                return HandleUnauthorized();
            }

            TempData["Error"] = defaultMessage;
            return RedirectToAction("Index");
        }

        protected bool IsUserLoggedIn()
        {
            return !string.IsNullOrEmpty(HttpContext.Session.GetString("Token"));
        }

        protected string GetCurrentUsername()
        {
            return HttpContext.Session.GetString("Username") ?? "";
        }

        protected int GetCurrentUserId()
        {
            return HttpContext.Session.GetInt32("UserId") ?? 0;
        }

        protected bool IsCurrentUserAdmin()
        {
            return HttpContext.Session.GetString("IsAdmin") == "True";
        }

        protected string GetCurrentUserFullName()
        {
            return HttpContext.Session.GetString("FullName") ?? "";
        }



        /// <summary>
        /// Verify user has Admin role, redirect if not
        /// </summary>
        protected IActionResult? VerifyAdminRole()
        {
            if (!IsUserLoggedIn())
            {
                return HandleUnauthorized();
            }

            if (!IsCurrentUserAdmin())
            {
                return HandleAccessDenied();
            }

            return null; // User is admin, continue
        }

        /// <summary>
        /// Verify user is authenticated, redirect if not
        /// </summary>
        protected IActionResult? VerifyAuthentication()
        {
            if (!IsUserLoggedIn())
            {
                return HandleUnauthorized();
            }

            return null; // User is authenticated, continue
        }
    }
}
