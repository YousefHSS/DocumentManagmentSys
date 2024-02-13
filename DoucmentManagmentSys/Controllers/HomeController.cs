using DoucmentManagmentSys.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using DoucmentManagmentSys.Repo;
using DoucmentManagmentSys.RoleManagment;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNet.Identity;
using System.Security.Claims;
using Microsoft.AspNetCore.Mvc.Routing;
using DoucmentManagmentSys.Models.Static;
using Microsoft.CodeAnalysis.CodeFixes;


namespace DoucmentManagmentSys.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;


        private readonly DocumentRepository _DocsRepo;

        private readonly IRoleManagment _roleManagment;

        public SignInManager<IdentityUser> _signInManager { get; set; }

        public HomeController(ILogger<HomeController> logger, DocumentRepository repository, IRoleManagment roleManagment, SignInManager<IdentityUser> signInManager)
        {
            _logger = logger;
            _DocsRepo = repository;
            _roleManagment = roleManagment;
            _signInManager = signInManager;
        }

        public IActionResult Index(string Message, string Messages)
        {

            ViewBag.Message = Message ?? "";
            ViewBag.Messages = Messages ?? "";
            TempData["Id"] = TempData["Id"] ?? "";

            return View(_DocsRepo.GetAll());
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile oFile)
        {
            MessageResult result = ServerFileManager.UploadtoServer(oFile);

            if (result.Status)
            {
                result = await SaveToDB();
            }

            return RedirectToAction("index", "Home", new { Message = result.Message });

        }



        public async Task<MessageResult> SaveToDB()
        {
            string strFolder = "./UploadedFiles/";
            MessageResult Result = _DocsRepo.AddRange(await ServerFileManager.FilesToDocs());

            if (Result.Status)
            {
                _DocsRepo.SaveChanges();
            }

            ViewBag.Message = Result.Message;
            ServerFileManager.CleanDirectory(strFolder);
            return Result;

        }

        public async Task<MessageResult> UpdateToDB(int id, string newName)
        {

            string strFolder = "./UploadedFiles/";
            MessageResult Result = _DocsRepo.Update(id, newName);
            if (Result.Status)
            {
                _DocsRepo.SaveChanges();
            }




            ServerFileManager.CleanDirectory(strFolder);
            return Result;


        }

        [HttpPost]
        public IActionResult DownloadFile(string name, int id)
        {
            string Message = "Download Started.";
            //Get name from database and then check



            Document document = _DocsRepo.Find([id, name]);
            if (document == null)
            {
                Message = "document not found";
                return RedirectToAction("index", "Home", new { Message });
            }
            else
            {

                return File(document.Content, FileTypes.GetContentType(document.FileName), document.FileName);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteFile(int id, string fileName)
        {
            Document document = _DocsRepo.Find([id, fileName]);
            if (document == null)
            {
                return Content("document not found");
            }
            else
            {
                _DocsRepo.Delete(document);
                _DocsRepo.SaveChanges();
                return RedirectToAction("index", "Home", new { Message = "File deleted successfully." });
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Update(IFormFile oFile, int id)
        {
            MessageResult result = ServerFileManager.UploadtoServer(oFile);

            if (result.Status)
            {
                result = await UpdateToDB(id, oFile.FileName);
            }
            ViewBag.Messages = result.Message;
            TempData["Id"] = id;

            return RedirectToAction("index", "Home", new { ViewBag.Messages });


            //return RedirectToAction("index", "Home", ViewBag.Message = "File updated successfully.");

        }

        public IActionResult Privacy()
        {
            return View();
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Confirmation(int id, string FileName)
        {
            ViewBag.id = id;
            ViewBag.FileName = FileName;
            return View("Confirmation");
        }


        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        //[HttpGet]
        //public async Task<IActionResult> AdminAsync()
        //{
        //    // Add admin logic here
        //    // Assuming you have a UserManager instance named "userManager"

        //    await _roleManagment.SwitchRole(User, "Admin");

        //    await _signInManager.SignOutAsync();

        //    return RedirectToAction("index", "Home");
        //}

        //[HttpGet]
        //public async Task<IActionResult> UserAsync()
        //{
        //    // Add admin logic here
        //    // Assuming you have a UserManager instance named "userManager"

        //    await _roleManagment.SwitchRole(User, "User");
        //    await _signInManager.SignOutAsync();
        //    return RedirectToAction("index", "Home");
        //}






    }
}
