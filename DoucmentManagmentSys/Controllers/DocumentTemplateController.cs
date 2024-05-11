using System.Collections.Generic;
using System.Collections.ObjectModel;
using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using DoucmentManagmentSys.Helpers.Word;
using DoucmentManagmentSys.Models;
using DoucmentManagmentSys.Models;
using DoucmentManagmentSys.Repo;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
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
            string[] strings = ["AssayMethodValidationProtocol"];
            return View(strings);
        }

        [HttpGet]

        public ActionResult GetCreationForm(string Title, int? page)
        {
            page ??= 1;
            //get from db the DocumentForm
            var DocumentTemplate = _DocumentTemplateRepo.GetWhere(x => x.Title == Title).FirstOrDefault();
            if (DocumentTemplate == null)
            {
                DocumentTemplate = WordTemplateHelper.CreateDocumentTemplate(Title);
            }
            var TemplateElements = DocumentTemplate.TemplateElements;
            List<TemplateElement> ListedElements;
            //get Template Elements List
            if(page == 1)
            {
                //get the ones with fixed title substance and strength
                ListedElements=TemplateElements.Where(TE=>TE.FixedTitle == "Substance" || TE.FixedTitle == "Strength").ToList();
            }
            else
            {
                //get according to the page and make sure to count the element from the if condition as it is the first page
                ListedElements = TemplateElements.Skip((page.Value-1)*10).Take(10).ToList();
            }
            ViewBag.TotalPages = TemplateElements.Count;
            ViewBag.Title = Title;
            return View("GetCreationForm", ListedElements);
        }



    }
}
