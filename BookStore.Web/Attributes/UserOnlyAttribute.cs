using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace BookStore.Web.Attributes
{
    public class UserOnlyAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var hasToken = !string.IsNullOrEmpty(context.HttpContext.Session.GetString("Token"));
            
            if (!hasToken)
            {
                context.Result = new RedirectToActionResult("Login", "Account", null);
                return;
            }

            base.OnActionExecuting(context);
        }
    }
}
