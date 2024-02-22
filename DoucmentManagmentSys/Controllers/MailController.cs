using CC.Web.Helpers;
using DoucmentManagmentSys.Models.Static;
using DoucmentManagmentSys.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace DoucmentManagmentSys.Controllers
{
    public class MailController : Controller
    {
        private readonly IEmailSender emailSender;

        public MailController(IEmailSender emailSender)
        {
            this.emailSender = emailSender;
        }

        public IActionResult Index()
        {

            return View();
        }

        public async Task<IActionResult> SendMailAsync(string Filename)
        {
            //Doc.Approve();
            //_DocsRepo.SaveChanges();
            string domain = $"{HttpContext.Request.Scheme}://{HttpContext.Request.Host}";
            MailModel mailModel = new MailModel($"{domain}{Url.Action("GSearch", "Home", new { search = Filename })}", "Approve");

            //MailModel mailModel = new MailModel(Url.+Url.Action("GSearch", "Home", new { search = Filename }), "Approve");
            ViewResult view = View("Index", mailModel);

            var body = ControllerExtensions.RenderViewAsync(this, "Index", mailModel, true).Result;
            await emailSender.SendEmailAsync("yousefhussen139@gmail.com", "Test", body);
            return RedirectToAction("index", "Home", new { Message = "Mail Sent." });
        }

    }
}
