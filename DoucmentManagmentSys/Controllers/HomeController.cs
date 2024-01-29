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


namespace DoucmentManagmentSys.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;


        private readonly IRepository<Document> mainRepo;

        private readonly IRoleManagment _roleManagment;

        public SignInManager<IdentityUser> _signInManager { get; set; }

        public HomeController(ILogger<HomeController> logger, IRepository<Document> repository, IRoleManagment roleManagment, SignInManager<IdentityUser> signInManager)
        {
            _logger = logger;
            mainRepo = repository;
            _roleManagment = roleManagment;
            _signInManager = signInManager;
        }

        public IActionResult Index()
        {
            IEnumerable<Document> documents = mainRepo.GetAll();
            return View(mainRepo.GetAll());
        }

        [HttpPost]
        public async Task<IActionResult> UploadFile(IFormFile oFile)
        {
            if (oFile != null)
            {
                //check the file type
                if (oFile.ContentType != "application/pdf")
                {
                    ViewBag.Message = "File type is not supported.";
                    return RedirectToAction("index", "Home");
                }
                string strFileName;
                string strFilePath;
                string strFolder;
                strFolder = "./UploadedFiles/";
                // Retrieve the name of the file that is posted.
                strFileName = oFile.FileName;
                strFileName = Path.GetFileName(strFileName);
                if (oFile.ContentDisposition != "")
                {

                    // Create the folder if it does not exist.
                    if (!Directory.Exists(strFolder))
                    {
                        Directory.CreateDirectory(strFolder);
                    }
                    // Save the uploaded file to the server.
                    strFilePath = strFolder + strFileName;
                    if (Path.Exists(strFilePath))
                    {
                        //need to be retrieved from database and then check
                        ViewBag.Message = "File Exits Already.";
                    }
                    else
                    {
                        using (FileStream fs = System.IO.File.Create(strFilePath))
                        {
                            oFile.CopyTo(fs);
                            fs.Flush();
                        }
                        await SaveFilesToDB();
                        ViewBag.Message = "File uploaded successfully.";
                    }
                }
                else
                {
                    ViewBag.Message = "File is empty.";
                }
                // Display the result of the upload.
                return RedirectToAction("index", "Home");
            }
            else
            {
                ViewBag.Message = "No File Selected.";
                return RedirectToAction("index", "Home");
            }

        }



        public async Task SaveFilesToDB()
        {
            string strFolder = "./UploadedFiles/";
            List<Document> documents = new List<Document>();

            if (Directory.Exists(strFolder))
            {
                string[] fileNames = Directory.GetFiles(strFolder);

                foreach (string fileName in fileNames)
                {
                    byte[] fileData = await System.IO.File.ReadAllBytesAsync(fileName);

                    Document document = new Document
                    {
                        FileName = Path.GetFileName(fileName),
                        Content = fileData
                    };

                    documents.Add(document);

                }

                // Save documents to the database
                // Your code to save documents to the database goes here
                int AddedDocuments = 0;
                AddedDocuments = mainRepo.AddRange(documents);
                mainRepo.SaveChanges();
                ViewBag.Message = AddedDocuments + " Files saved to database successfully.";
            }
            else
            {
                ViewBag.Message = "No files found in the UploadedFiles folder.";
            }
            //Delete all inside the folder

            Array.ForEach(Directory.GetFiles(strFolder), System.IO.File.Delete);





        }
        [HttpPost]
        public async Task<IActionResult> DownloadFile(int id)
        {
            Document document = mainRepo.GetById(id);
            if (document == null)
            {
                return Content("document not found");
            }
            else
            {
                return File(document.Content, "application/pdf", document.FileName);
            }
        }
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteFile(int id, string fileName)
        {
            Document document = mainRepo.Find([id, fileName]);
            if (document == null)
            {
                return Content("document not found");
            }
            else
            {
                mainRepo.Delete(document);
                mainRepo.SaveChanges();
                return RedirectToAction("index", "Home", ViewBag.Message = "File deleted successfully.");
            }
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

        [HttpGet]
        public async Task<IActionResult> AdminAsync()
        {
            // Add admin logic here
            // Assuming you have a UserManager instance named "userManager"

            await _roleManagment.AssignRole(User, "Admin");

            await _signInManager.SignOutAsync();

            return RedirectToAction("index", "Home");
        }

        [HttpGet]
        public async Task<IActionResult> UserAsync()
        {
            // Add admin logic here
            // Assuming you have a UserManager instance named "userManager"

            await _roleManagment.AssignRole(User, "User");
            await _signInManager.SignOutAsync();
            return RedirectToAction("index", "Home");
        }






    }
}
