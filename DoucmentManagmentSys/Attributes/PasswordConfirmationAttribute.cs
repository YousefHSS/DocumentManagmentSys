using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;

namespace DoucmentManagmentSys.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PasswordConfirmationAttribute : Attribute, IAsyncActionFilter
    {
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            if (!context.HttpContext.Session.TryGetValue("PasswordValidated", out _))
            {
                var formDataJson = System.Text.Json.JsonSerializer.Serialize(context.ActionArguments);

                context.Result = new RedirectToActionResult("PreConfirmAction", "Confirmation", new { returnUrl = context.HttpContext.Request.Path, formData= formDataJson });
                return;
            }

            await next();
            context.HttpContext.Session.Remove("PasswordValidated");
            //context.Result = new RedirectToActionResult("Home", "Index", new { Message = "Password Validated" });


        }
    }
}
