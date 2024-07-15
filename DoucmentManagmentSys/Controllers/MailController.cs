using CC.Web.Helpers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.UI.Services;
using DoucmentManagmentSys.RoleManagment;
using DoucmentManagmentSys.Models;
using DoucmentManagmentSys.Models;

namespace DoucmentManagmentSys.Controllers
{
    public class MailController : Controller
    {
        private readonly IEmailSender emailSender;

        private readonly IRoleManagment _roleManagment;


        public MailController(IEmailSender emailSender, IRoleManagment roleManagment)
        {
            this.emailSender = emailSender;
            this._roleManagment = roleManagment;
        }

        public IActionResult Index()
        {

            return View();
        }

        public async Task<IActionResult> SendMailAsync(string Filename, string actionTaken, PrimacyDocument.Status status, string? reason)
        {
            // Get the domain from the current HTTP context
            string domain = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";

            // Create a new MailModel with the search URL and action
            MailModel mailModel = new MailModel($"{domain}{Url.Action("GSearch", "Home", new { search = Filename })}", actionTaken, reason);

            // Render the view for the mail model
            ViewResult view = View("Index", mailModel);

            // Get the body of the email by rendering the view asynchronously
            var body = ControllerExtensions.RenderViewAsync(this, "Index", mailModel, true).Result;

            // Default role is "Uploader"
            var role = "Uploader";

            // Determine the role based on the document status
            switch (status)
            {
                case PrimacyDocument.Status.Under_Revison:
                    role = "Revisor";
                    break;
                case PrimacyDocument.Status.Under_Finalization:
                    role = "Finalizer";
                    break;
            }

            // Get the users with the determined role
            string[] users = _roleManagment.GetUsersByRole(role);

            // Send an email to each user
            foreach (var user in users)
            {
                try
                {
                    await emailSender.SendEmailAsync(user, Filename + " has been " + actionTaken, body);
                }
                catch
                {
                    Console.WriteLine("could not send mails server is not connected to internet");
                }
                
            }

            // Redirect to the home page with a success message
            return RedirectToAction("InProcess", "Home", new { Message = "Mail Sent." });
        }

    }
}
