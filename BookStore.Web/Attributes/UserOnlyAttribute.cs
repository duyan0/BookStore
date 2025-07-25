using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookStore.Web.Attributes
{
    public class UserOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var hasToken = !string.IsNullOrEmpty(context.HttpContext.Session.GetString("Token"));
            var isAdmin = context.HttpContext.Session.GetString("IsAdmin") == "True";

            if (!hasToken)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            // Redirect admin users to admin panel
            if (isAdmin)
            {
                context.Result = new RedirectToActionResult("Index", "Home", new { area = "Admin" });
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
