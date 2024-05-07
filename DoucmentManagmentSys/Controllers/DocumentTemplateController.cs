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
        private readonly MainRepo<DocumentTemplate> _DocumentFormRepo;

        private readonly SignInManager<PrimacyUser> _signInManager;

        public DocumentTemplateController(MainRepo<DocumentTemplate> DocumentFormRepo, SignInManager<PrimacyUser> signInManager)
        {
            _signInManager = signInManager;
            _DocumentFormRepo = DocumentFormRepo;
        }
        public IActionResult CreateForm()
        {
            string[] strings = ["Assay Method Validation Protocol"];
            return View(strings);
        }

        [HttpPost]
        public IActionResult GetCreationForm(string Title)
        {
            //get from db the DocumentForm
            var DocumentForm = _DocumentFormRepo.GetWhere(x => x.Title == Title).FirstOrDefault();
            if (DocumentForm == null)
            {
                /*
                //create an open xml element with type text and type table
                var elementText = new Text("This is a test");
                var elementTable = new Table(
                      new TableProperties(
                          new TableStyle
                          {
                              Val = "TableGrid"
                          }
                      ),
                      new TableGrid(
                          new GridColumn(),
                          new GridColumn(),
                          new GridColumn(),
                          new GridColumn()
                          ),
                      new TableRow( new TableCell(new Text("Row1 Test cell1")), new TableCell(new Text("Row1 Test cell2")), new TableCell(new Text("Row1 Test cell3")), new TableCell(new Text("Row1 Test cell4"))),
                      new TableRow(new TableCell(new Text("Row2 Test cell1")), new TableCell(new Text("Row2 Test cell2")), new TableCell(new Text("Row2 Test cell3")), new TableCell(new Text("Row2 Test cell4"))),
                      new TableRow(new TableCell(new Text("Row3 Test cell1")), new TableCell(new Text("Row3 Test cell2")), new TableCell(new Text("Row3 Test cell3")), new TableCell(new Text("Row3 Test cell4"))),
                      new TableRow(new TableCell(new Text("Row4 Test cell1")), new TableCell(new Text("Row4 Test cell2")), new TableCell(new Text("Row4 Test cell3")), new TableCell(new Text("Row4 Test cell4")))
                    );

                //create a new DocumentForm
                var newDocumentForm = new DocumentTemplate()
                {
                    Title = Title,
                    CreatedBy = PrimacyUser.GetCurrentUserName(_signInManager, User.Identity.Name).Result,
                    CreatedAt = DateTime.Now,
                    FixedElements = new Collection<TemplateElement>
                        {
                            new TemplateElement
                            {
                                FixedTitle = "This is a test Text",
                                Element = elementText
                            },
                            new TemplateElement
                            {
                                FixedTitle= "This is test Table",
                                Element= elementTable
                            }

                        }

                
                };*/
                var newDocumentForm = WordTemplateHelper.CreateDocumentTemplate(Title);
                return PartialView(newDocumentForm);
            }
            return PartialView();
        }

    }
}
