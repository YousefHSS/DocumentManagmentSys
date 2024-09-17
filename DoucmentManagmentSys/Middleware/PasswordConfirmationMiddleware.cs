using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using System.Threading.Tasks;

namespace DoucmentManagmentSys.Middleware
{
    // You may need to install the Microsoft.AspNetCore.Http.Abstractions package into your project
    public class PasswordConfirmationMiddleware
    {
        private readonly RequestDelegate _next;

        public PasswordConfirmationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            if (context.Request.Path.StartsWithSegments("/Home/DeleteFile") && context.Request.Method == "POST")
            {
                if (!context.Session.TryGetValue("PasswordValidated", out _))
                {
                    context.Response.Redirect("/Home/ConfirmPassword");
                    return;
                }

                context.Session.Remove("PasswordValidated");
            }

            await _next(context);
        }
    }
}
