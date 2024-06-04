using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text.Json.Nodes;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using DoucmentManagmentSys.Helpers.Word;
using DoucmentManagmentSys.Models;
using DoucmentManagmentSys.Models;
using DoucmentManagmentSys.Repo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Table = DocumentFormat.OpenXml.Wordprocessing.Table;
using TableStyle = DocumentFormat.OpenXml.Wordprocessing.TableStyle;
using Text = DocumentFormat.OpenXml.Wordprocessing.Text;

namespace DoucmentManagmentSys.Controllers
{
    public class DocumentTemplateController : Controller
    {
        private readonly MainRepo<DocumentTemplate> _DocumentTemplateRepo;

        private readonly SignInManager<PrimacyUser> _signInManager;

        public DocumentTemplateController(MainRepo<DocumentTemplate> DocumentFormRepo, SignInManager<PrimacyUser> signInManager)
        {
            _signInManager = signInManager;
            _DocumentTemplateRepo = DocumentFormRepo;
        }
        public IActionResult CreateForm()
        {
            string[] strings = ["Assay Method Validation Protocol"];
            return View(strings);
        }
        [HttpPost]

        public ActionResult GetCreationForm(string TemplateTitle, int? page, List<TemplateElement>? newTemplateElements, int? lastPage, string? ToBeJson)
        {

            //get from db the DocumentForm along with its TemplateElements
            var DocumentTemplate = _DocumentTemplateRepo.GetDbSet() // Get the DbSet directly
                             .Include(dt => dt.TemplateElements)
                             .FirstOrDefault(x => x.Title == TemplateTitle);
            if (DocumentTemplate == null || DocumentTemplate.TemplateElements == null)
            {
                DocumentTemplate = WordTemplateHelper.CreateDocumentTemplate(TemplateTitle);
                _DocumentTemplateRepo.Add(DocumentTemplate);
                _DocumentTemplateRepo.SaveChanges();
            }
            if (ToBeJson != null && lastPage != null)
            {
                DocumentTemplate = WordTemplateHelper.UpdateDocumentTemplate(DocumentTemplate, ToBeJson, lastPage - 1);
                _DocumentTemplateRepo.SaveChanges();
            }
            var TemplateElements = DocumentTemplate.TemplateElements;
            List<TemplateElement> ListedElements;
            //get Template Elements List
            page = page == null ? 0 : page - 1;
            if (page == 0)
            {
                //get the ones with fixed title substance and strength
                ListedElements = TemplateElements.Where(TE => TE.FixedTitle == "Substance" || TE.FixedTitle == "Strength").ToList();
            }
            else
            {
                DocumentTemplate.PreProcessTemplateElements();
                //remove the ones with fixed title substance and strength then get the rest as in pages from 2 onwards
                var TemplateElements2 = TemplateElements.Where(TE => TE.FixedTitle != "Substance" && TE.FixedTitle != "Strength").ToList();
                ListedElements = TemplateElements2.Skip((page.Value) - 1).Take(1).ToList();
            }
            ViewBag.TotalPages = TemplateElements.Count - 1;
            ViewBag.TemplateTitle = TemplateTitle;
            ViewBag.CurrentPage = page.Value + 1;
            return View("GetCreationForm", ListedElements);
        }



        [HttpPost]
        public ActionResult SaveDocument(string TemplateTitle, int lastPage, string ToBeJson)
        {
            var DocumentTemplate = _DocumentTemplateRepo.GetDbSet() // Get the DbSet directly
                 .Include(dt => dt.TemplateElements)
                 .FirstOrDefault(x => x.Title == TemplateTitle);
            DocumentTemplate = WordTemplateHelper.UpdateDocumentTemplate(DocumentTemplate, ToBeJson, lastPage - 1);
            _DocumentTemplateRepo.SaveChanges();
            //create a new primacy document from the template
            AssayMethodValidationProtocolTemplate.ImportTemplateElements(DocumentTemplate.TemplateElements.ToList());
            //go to a new controller 
            return RedirectToAction("Index", "Home");

        }
    }
}
