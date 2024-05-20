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
        private readonly MainRepo<AssayMethodValidationProtocolTemplate> _DocumentTemplateRepo;

        private readonly SignInManager<PrimacyUser> _signInManager;

        public DocumentTemplateController(MainRepo<AssayMethodValidationProtocolTemplate> DocumentFormRepo, SignInManager<PrimacyUser> signInManager)
        {
            _signInManager = signInManager;
            _DocumentTemplateRepo = DocumentFormRepo;
        }
        public IActionResult CreateForm()
        {
            string[] strings = ["AssayMethodValidationProtocol"];
            return View(strings);
        }

        [HttpPost]

        public ActionResult GetCreationForm(string TemplateTitle, int? page,List<TemplateElement>? newTemplateElements)
        {
            page ??= 1;
            //get from db the DocumentForm
            var DocumentTemplate = _DocumentTemplateRepo.GetWhere(x => x.Title == TemplateTitle).FirstOrDefault();
            if (DocumentTemplate == null)
            {
                DocumentTemplate = WordTemplateHelper.CreateDocumentTemplate(TemplateTitle);
            }
            var TemplateElements = DocumentTemplate.TemplateElements;
            List<TemplateElement> ListedElements;
            //get Template Elements List
            page = page - 1;
            if (page == 0)
            {
                //get the ones with fixed title substance and strength
                ListedElements=TemplateElements.Where(TE=>TE.FixedTitle == "Substance" || TE.FixedTitle == "Strength").ToList();
            }
            else
            {
                DocumentTemplate.PreProcessTemplateElements();
                //remove the ones with fixed title substance and strength then get the rest as in pages from 2 onwards
                var TemplateElements2 = TemplateElements.Where(TE => TE.FixedTitle != "Substance" && TE.FixedTitle != "Strength").ToList();
                ListedElements = TemplateElements2.Skip((page.Value)-1).Take(1).ToList();
            }
            ViewBag.TotalPages = TemplateElements.Count-1;
            ViewBag.TemplateTitle = TemplateTitle;
            ViewBag.CurrentPage = page.Value+ 1;
            return View("GetCreationForm", ListedElements);
        }



    }
}
