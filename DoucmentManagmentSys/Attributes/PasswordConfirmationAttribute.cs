using Microsoft.AspNetCore.Mvc.Filters;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using Microsoft.DotNet.Scaffolding.Shared.Messaging;
using DoucmentManagmentSys.Models;

namespace DoucmentManagmentSys.Attributes
{
    [AttributeUsage(AttributeTargets.Method)]
    public class PasswordConfirmationAttribute : Attribute, IAsyncActionFilter
    {
        
        
        public async Task OnActionExecutionAsync(ActionExecutingContext context, ActionExecutionDelegate next)
        {
            
            var  valu = context.RouteData.Values;
            var RouteData = context.HttpContext.Request.Form.Where(Pair => Pair.Key == "password").FirstOrDefault();
            var value = RouteData.Value.ToString();

        
            if (value == null)
            {
                context.Result = new RedirectToActionResult((string?)context.RouteData.Values["action"], (string?)context.RouteData.Values["Controller"], new { Message = "Error: Password Required" });
                return;
            }

            var password = value as string;
            if (string.IsNullOrEmpty(password))
            {
                
                context.Result = new RedirectToActionResult("InProcess","Home" , new { Message = "Error: Password Required" });
                return;
            }

            // Resolve the service from the validation context
            var signInManager = context.HttpContext.RequestServices.GetService(typeof(SignInManager<PrimacyUser>)) as SignInManager<PrimacyUser>;
            
            var userManager = context.HttpContext.RequestServices.GetService<UserManager<PrimacyUser>>();

            if (signInManager == null)
            {
                throw new InvalidOperationException("SignInManager service is not available.");
            }

            // Replace this with your actual password validation logic
            var user = await userManager.FindByNameAsync(context.HttpContext.User.Identity.Name);
            
            var result = await signInManager.CheckPasswordSignInAsync(user, password, false);


            if (!result.Succeeded)
            {
                context.Result = new RedirectToActionResult((string?)context.RouteData.Values["controller"], (string?)context.RouteData.Values["action"], new { Message = "Error: Password Invalid" });
                return;
            }

            await next();



        }
    }
}
