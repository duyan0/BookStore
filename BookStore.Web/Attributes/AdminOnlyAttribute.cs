using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookStore.Web.Attributes
{
    /// <summary>
    /// Custom authorization attribute that requires Admin role
    /// </summary>
    public class AdminOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            
            // Check if user is authenticated
            var token = httpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Check if user has Admin role
            var isAdmin = httpContext.Session.GetString("IsAdmin") == "True";
            if (!isAdmin)
            {
                // Return 403 Forbidden for non-admin users
                context.Result = new ContentResult
                {
                    StatusCode = 403,
                    Content = "Access Denied: Chỉ Admin mới có quyền truy cập trang này.",
                    ContentType = "text/html; charset=utf-8"
                };
                return;
            }

            base.OnActionExecuting(context);
        }
    }

    /// <summary>
    /// Custom authorization attribute that requires authentication only
    /// </summary>
    public class AuthenticatedOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var httpContext = context.HttpContext;
            
            // Check if user is authenticated
            var token = httpContext.Session.GetString("Token");
            if (string.IsNullOrEmpty(token))
            {
                var returnUrl = httpContext.Request.Path + httpContext.Request.QueryString;
                context.Result = new RedirectToActionResult("Login", "Account", 
                    new { returnUrl = returnUrl });
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
