using DocumentFormat.OpenXml.Drawing;
using DoucmentManagmentSys.Helpers.Word;
using DoucmentManagmentSys.Models;
using DoucmentManagmentSys.Repo;
using DoucmentManagmentSys.RoleManagment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DoucmentManagmentSys.Controllers
{
    // only accessable by admin

    public class DashboardController : Controller
    {
        private readonly MainRepo<PrimacyDocument> _DocumentRepo;
        private readonly IRoleManagment _roleManagment;
        private readonly UserManager<PrimacyUser> _userManager;

        public DashboardController(MainRepo<PrimacyDocument> DocumentRepo, UserManager<PrimacyUser> userManager, IRoleManagment roleManagment)
        {
            _userManager = userManager;
            _roleManagment = roleManagment;
            _DocumentRepo = DocumentRepo;
        }

        // Action to handle role changes
        [HttpPost]
        public async Task<bool> ChangeRole(string userId, string newRole)
        {
            // Get the user
            var user = _userManager.FindByIdAsync(userId).Result;
            var Result = false;
            if (user != null)
            {
                Result = await _roleManagment.SwitchRole(user, newRole);
            }
            return Result;
        }

        [HttpPost]
        public ActionResult CheckCodesIntegrity()
        {
            // Fetch documents with null Code values
            var documentsWithNullCode = _DocumentRepo.GetWhere(doc => doc.Code == null).ToList();
            bool integrityCheckPassed = true;

            foreach (var document in documentsWithNullCode)
            {
                // Extract the Code from the document
                var extractedCode = ExtractCodeFromDocument(document);

                if (extractedCode != null && !_DocumentRepo.GetWhere(doc => doc.Code == extractedCode).Any())
                {
                    // If the extracted code is unique, update the document
                    document.Code = extractedCode;
                    _DocumentRepo.Update(document);
                }
                else if (extractedCode == null)
                {
                    Console.WriteLine("Document without code: " + document.FileName);
                    
                }
                else
                {
                    Console.WriteLine("Code integrity check failed for document: " + document.FileName);
                    integrityCheckPassed = false;
                }
            }

            // Save changes to the repository
            _DocumentRepo.SaveChanges();

            if (integrityCheckPassed)
            {
                return RedirectToAction("index", "Home", new { Message = "Code integrity check passed. All documents have been updated." });

            }
            else
            {
                return RedirectToAction("index", "Home", new { Message = "Code integrity check failed. Please try again." });
            }
        }

        // Action to display the dashboard
        [Authorize(Roles = "Admin")]
        public ActionResult index()
        {
            // Get the list of users from your data source
            // Get the currently authenticated user
            var currentUser = _userManager.GetUserAsync(User).Result;

            // Get the list of all users
            var users = _userManager.Users.ToList();

            ViewData["Title"] = "Dashboard";
            return View(users);
        }

        // Placeholder method to extract Code from document
        private string ExtractCodeFromDocument(PrimacyDocument document)
        {
            WordDocumentHelper wordDocumentHelper = new WordDocumentHelper(document);

            return wordDocumentHelper.ExtractCode();
        }
    }
}