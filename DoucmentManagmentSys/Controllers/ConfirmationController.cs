using DocumentFormat.OpenXml.InkML;
using DoucmentManagmentSys.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DoucmentManagmentSys.Controllers
{
    public class ConfirmationController : Controller
    {
        private readonly SignInManager<PrimacyUser> _signInManager;
        private readonly UserManager<PrimacyUser> _userManager;
        public ConfirmationController(SignInManager<PrimacyUser> signInManager, UserManager<PrimacyUser> userManager)
        {
            _signInManager = signInManager;
            _userManager = userManager;
        }
        public IActionResult ConfirmAction(string Action,string Controller)
        {
            return View(model:new { Action, Controller });
        }

        public IActionResult PreConfirmAction(string returnUrl, string formData)
        {
            var formDataDictionary = System.Text.Json.JsonSerializer.Deserialize<IDictionary<string, object?>>(formData);

            TempData["formData"] = formData;
            //TempData["method"] = method;
            TempData.Keep("formData");
            TempData.Keep("method");
            return new RedirectToActionResult("ConfirmPassword", "Confirmation", new { ReturnUrl = returnUrl });
        }

        [HttpGet]
        public IActionResult ConfirmPassword(string returnUrl)
        {

            ViewData["ReturnUrl"] = returnUrl;
            TempData.Keep("formData");
            TempData.Keep("ReturnUrl");
            TempData.Keep("method");
            return View();
        }

        [HttpPost]
        public IActionResult ConfirmPassword(string password, string returnUrl)
        {
            if (ValidatePassword(password).Result)
            {
                TempData.Keep("formData");


                return RedirectToAction("RedirectToReturnUrl", new { returnUrl });
            }

            TempData["Error"] = "Invalid password";
            return View();
        }

        public IActionResult RedirectToReturnUrl(string returnUrl)
        {
            var formData = TempData["formData"];

            if (string.IsNullOrEmpty(returnUrl) || formData == null)
            {
                return BadRequest();
            }

            ViewData["ReturnUrl"] = returnUrl;
            ViewData["formData"] = formData;
            HttpContext.Session.SetString("PasswordValidated", "true");


            return View();
        }

        private async Task<bool> ValidatePassword(string password)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null)
            {
                return false;
            }

            var result = await _signInManager.CheckPasswordSignInAsync(user, password, lockoutOnFailure: false);

            if (result.Succeeded)
            {
                

                // Return a view with JavaScript to redirect back to the previous page
                return true;
            }

            return false;
        }

    }
}
