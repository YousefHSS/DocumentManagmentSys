using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using DoucmentManagmentSys.Data;
using DoucmentManagmentSys.Repo;
using Microsoft.AspNetCore.Authorization;
using DocumentFormat.OpenXml.EMMA;
using DoucmentManagmentSys.Helpers;
using DoucmentManagmentSys.Models;

namespace DoucmentManagmentSys.Controllers
{
    [Authorize]
    public class ArchivedDocumentsController : Controller
    {
        private readonly MainRepo<ArchivedDocument> _ArchivedDocumentRepo;





        public ArchivedDocumentsController( MainRepo<ArchivedDocument> archivedDocumentRepo)
        {
            _ArchivedDocumentRepo = archivedDocumentRepo;
        }
       

        // GET: ArchivedDocuments
        public  IActionResult Index()
        {
            // Ensure the Document is included
            
            return View(_ArchivedDocumentRepo.GetAll());
        }

        // GET: ArchivedDocuments/Details/5
        public IActionResult Details(int id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var archivedDocument =  _ArchivedDocumentRepo.Find(id);
            if (archivedDocument == null)
            {
                return NotFound();
            }

            return View(archivedDocument);
        }

        // GET: ArchivedDocuments/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: ArchivedDocuments/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Create([Bind("Id")] ArchivedDocument archivedDocument)
        {
            if (ModelState.IsValid)
            {
                _ArchivedDocumentRepo.Add(archivedDocument);
                _ArchivedDocumentRepo.SaveChanges();
                return RedirectToAction(nameof(Index));
            }
            return View(archivedDocument);
        }

        // GET: ArchivedDocuments/Edit/5
        public IActionResult Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var archivedDocument =  _ArchivedDocumentRepo.Find(id);
            if (archivedDocument == null)
            {
                return NotFound();
            }
            return View(archivedDocument);
        }

        // POST: ArchivedDocuments/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Edit(int id, [Bind("Id")] ArchivedDocument archivedDocument)
        {
            if (id != archivedDocument.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _ArchivedDocumentRepo.Update(archivedDocument);
                     _ArchivedDocumentRepo.SaveChanges();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ArchivedDocumentExists(archivedDocument.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(archivedDocument);
        }

        // GET: ArchivedDocuments/Delete/5
        public IActionResult Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var archivedDocument = _ArchivedDocumentRepo
                .Find(id);
            if (archivedDocument == null)
            {
                return NotFound();
            }

            return View(archivedDocument);
        }

        // POST: ArchivedDocuments/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteConfirmed(int id)
        {
            var archivedDocument =  _ArchivedDocumentRepo.Find(id);
            if (archivedDocument != null)
            {
                _ArchivedDocumentRepo.Delete(archivedDocument);
            }

             _ArchivedDocumentRepo.SaveChanges();
            return RedirectToAction(nameof(Index));
        }

        private bool ArchivedDocumentExists(int id)
        {
            return _ArchivedDocumentRepo.GetWhere(e => e.Id == id).Any();
        }

        public List<string> GetVersions(int doc_id)
        {
            //Get the version of each ArchivedVersion in a list
            List<ArchivedVersion> versions = _ArchivedDocumentRepo.GetWhere(e => e.Id == doc_id)
                                                      .SelectMany(e => e.Versions)
                                                      .ToList();
            //get the version string and return it in an array of strings
            return versions.Select(v => v.Version).ToList();

        }

        [HttpPost, ActionName("download")]
        [Authorize]
        public IActionResult download(string version, int id)
        {
            string Message = "Download Started.";
            //Get name from database and then check



            ArchivedDocument document = _ArchivedDocumentRepo.Find(id);

            if (document == null)
            {
                Message = "document not found";
                return RedirectToAction("index", "Home", new { Message });
            }
            else
            {

                if (version == "Latest")
                {
                  //get the latest version
                    version = document.Versions.OrderByDescending(v => v.Version).FirstOrDefault().Version;
                }
                return File( document.Versions.Where(v => v.Version == version).FirstOrDefault().Content , FileTypes.GetContentType(document.FileName + document.Extension), document.FileName + document.Extension);
            }
        }

        [Authorize]
        [HttpGet]

        public IActionResult GSearch(string search)
        {
            if (search == null || search == string.Empty)
            {
                return View("index", _ArchivedDocumentRepo.GetAll());
            }
            else
            {
                return View("index", _ArchivedDocumentRepo.Search(search, "FileName"));
            }
        }

    }


}
