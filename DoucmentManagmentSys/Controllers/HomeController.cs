using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using DoucmentManagmentSys.Repo;
using DoucmentManagmentSys.RoleManagment;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNet.Identity;
using System.Reflection;
using DoucmentManagmentSys.Helpers;
using DoucmentManagmentSys.Helpers.Word;
using DoucmentManagmentSys.Models;
using Mammoth;
using DocumentFormat.OpenXml.Packaging;
using OpenXmlPowerTools;
using System.Xml.Linq;
using NPOI.HPSF;
using Spire.Doc.Documents;
using DoucmentManagmentSys.Attributes;




namespace DoucmentManagmentSys.Controllers
{
    public class HomeController : Controller
    {

        private readonly ILogger<HomeController> _logger;



        private readonly DocumentRepository _DocsRepo;

        private readonly MainRepo<HistoryAction> _HistoryActionRepo;

        private readonly MainRepo<HistoryLog> _HistoryLogRepo;

        private readonly MainRepo<ArchivedDocument> _ArchivedDocumentRepo;

        private readonly IRoleManagment _roleManagment;

        public SignInManager<PrimacyUser> _signInManager { get; set; }

        public HomeController(ILogger<HomeController> logger, DocumentRepository repository, IRoleManagment roleManagment, SignInManager<PrimacyUser> signInManager, MainRepo<HistoryAction> HistoryActionRepo, MainRepo<HistoryLog> HistoryLogRepo, MainRepo<ArchivedDocument> ArchivedDocumentRepo)
        {
            _logger = logger;
            _DocsRepo = repository;
            _roleManagment = roleManagment;
            _signInManager = signInManager;
            _HistoryActionRepo = HistoryActionRepo;
            _HistoryLogRepo = HistoryLogRepo;
            _ArchivedDocumentRepo = ArchivedDocumentRepo;


        }

        [Authorize]
        public IActionResult Index(string Message , string Messages)
        {

            return RedirectToAction("InProcess", "Home", new { Message , Messages });
        }


        [HttpGet]
        [Authorize]
        public IActionResult InProcess(string Message, string Messages, string? SortBY)
        {
            
            ViewData["Message"] = Message ?? "";
            ViewData["Messages"] = Messages ?? "";
            TempData["Id"] = TempData["Id"] ?? "";
            
            return View(
                    CheckCodeConflicts(SortBY != null ? OrderByProperty(_DocsRepo.GetAll(), SortBY) : _DocsRepo.GetAll())
                );
        }

        [HttpPost]
        [Authorize(Roles = "Uploader,Revisor")]
        public async Task<IActionResult> UploadFile(IFormFile oFile, string SuperCode, string SubCode, string SubVersionCode = "000", string VersionCode = "000")
        {
            string FullCode = SuperCode + "-" + SubCode + SubVersionCode + "-" + VersionCode;
            if (!WordDocumentHelper.IsValidCode(FullCode))
            {
                return RedirectToAction("InProcess", "Home", new { Message = "Code is not valid." });
            }
            MessageResult result = ServerFileManager.UploadtoServer(oFile);

            if (result.Status)
            {
                result = await SaveToDB();
                if (result.Info != null)
                {
                    foreach (Tuple<int, string> item in result.Info)
                    {

                        var Doc = _DocsRepo.GetWhere(x => (x.FileName == item.Item2) && (x.Id == item.Item1)).FirstOrDefault();
                        if (Doc == null)
                        {
                            continue;
                        }
                        WordDocumentHelper wordDocumentHelper = new WordDocumentHelper(Doc);
                        Doc.Code = FullCode;
                        _DocsRepo.Update(Doc);
                        _DocsRepo.SaveChanges();

                        var ExistingCode = wordDocumentHelper.ExtractCode();
                        if (ExistingCode != null && ExistingCode != FullCode)
                        {
                            var model = new Tuple<PrimacyDocument, string, string>(Doc, ExistingCode, FullCode);
                            return View("ChooseCode", model);
                        }
                        
                        
                    }
                }
            }
            
            return RedirectToAction("InProcess", "Home", new { Message = result.Message });
        }

        private IEnumerable<PrimacyDocument> CheckCodeConflicts(IEnumerable<PrimacyDocument> docs, MessageResult? result = null)
        {
            var docGroups = docs.GroupBy(d => new { d.Code, d.FileName });
            var conflictedDocs = new List<PrimacyDocument>();

            foreach (var group in docGroups)
            {
                if (group.Count() > 1)
                {
                    
                    
                    var oldestDoc = group.OrderBy(d => d.CreatedAt).First();
                    oldestDoc.Conflict = group.Count();
                    

                    conflictedDocs.Add(oldestDoc);
                }
                else
                {
                    conflictedDocs.Add(group.Single());
                } 
            }

            return conflictedDocs;
        }

        [HttpGet]
        public IActionResult ResolveConflict(string fileName, string code)
        {
            //gett alll docs wiith this code and this file name
            var documents = _DocsRepo.GetWhere(x => (x.FileName == fileName) && (x.Code == code));
            //gettt first accion of each docc from repo
            foreach (var doc in documents)
            {
                var log = _HistoryLogRepo.GetWhere(x => x.Document_id.Id == doc.Id).FirstOrDefault();
                if (log != null)
                {
                    var acion = _HistoryActionRepo.GetWhere(x => x.historyLog.id == log.id).FirstOrDefault();

                    doc.CreatorName = acion.UserName;
                }
            }
            return View(documents);

        }

        [HttpPost]
        [PasswordConfirmationAttribute]
        public IActionResult ResolveConflict(string Id)
        {
           //soft delete all docs with this name and code except the one with this id
           var lllleave = _DocsRepo.GetWhere(x => x.Id == int.Parse(Id)).FirstOrDefault();
            var docs = _DocsRepo.GetWhere(x => x.Id != int.Parse(Id) && (x.Code == lllleave.Code) && (x.FileName == lllleave.FileName));
            foreach (var doc in docs)
            {
                _DocsRepo.Delete(doc);
            }
            _DocsRepo.SaveChanges();
            return RedirectToAction("InProcess", "Home", new { Message = "conflicts resolved." });

        }

        [HttpGet]
        public IActionResult UseCode(int id, string fileName, string code)
        {
            var document = _DocsRepo.Find([id, fileName]);
            if (document != null)
            {
                document.Code = code;
                _DocsRepo.Update(document);
                _DocsRepo.SaveChanges();
            }
            return RedirectToAction("InProcess", "Home", new { Message = "code applied." });
        }

        



        public async Task<MessageResult> SaveToDB()
        {
            string strFolder = "./UploadedFiles/";
            List<PrimacyDocument> documents = await ServerFileManager.FilesToDocs(User.Identity.GetUserId());
            MessageResult Result = _DocsRepo.AddRange(documents);

            if (Result.Status)
            {
                AuditLogHelper.AddLogThenProcced(HistoryAction.Created, documents[0], _HistoryLogRepo, _HistoryActionRepo, PrimacyUser.GetCurrentUserName(_signInManager, User.Identity.Name!).Result);

                _DocsRepo.SaveChanges();
            }

            ViewData["Message"] = Result.Message;
            ServerFileManager.CleanDirectory(strFolder);
            return Result;

        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [PasswordConfirmationAttribute]
        public IActionResult Update(IFormFile oFile, int id)
        {
            MessageResult result = ServerFileManager.UploadtoServer(oFile);

            if (result.Status)
            {
                result = UpdateToDB(id, oFile.FileName);
            }
            ViewData["Message"] = result.Message;

            return RedirectToAction("index", "Home", new { Message = result.Message });

        }

        public MessageResult UpdateToDB(int id, string newName)
        {
            string strFolder = "./UploadedFiles/";
            MessageResult Result = _DocsRepo.Update(id, newName);

            if (Result.Status)
            {
                var Doc = _DocsRepo.Find([id, newName]);
                Doc.UpdateDocument();

                AuditLogHelper.AddLogThenProcced(HistoryAction.Updated, newName, id, _DocsRepo, _HistoryLogRepo, _HistoryActionRepo, PrimacyUser.GetCurrentUserName(_signInManager, User.Identity.Name!).Result);

                _DocsRepo.SaveChanges();
            }

            ServerFileManager.CleanDirectory(strFolder);
            return Result;
        }

        [HttpPost]
        [Authorize]
        public IActionResult DownloadFile(string name, int id)
        {
            string Message = "Download Started.";
            //Get name from database and then check


            PrimacyDocument document = _DocsRepo.Find([id, name]);
            if (document == null)
            {
                Message = "document not found";
                return RedirectToAction("index", "Home", new { Message });
            }
            else
            {
                AuditLogHelper.AddLogThenProcced(HistoryAction.Downloaded, name, id, _DocsRepo, _HistoryLogRepo, _HistoryActionRepo, PrimacyUser.GetCurrentUserName(_signInManager, User.Identity.Name!).Result);


                return File(document.Content, FileTypes.GetContentType(document.FileName + document.FileExtensiton), document.FileName + document.FileExtensiton);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Uploader,Revisor")]
        
        public IActionResult DeleteFile(int id, string fileName)
        {

            PrimacyDocument document = _DocsRepo.Find([id, fileName]);

            if (document == null)
            {
                return Content("document not found");
            }
            else
            {
                if (document.Creator == User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value)
                {
                    _DocsRepo.Delete(document);
                    _DocsRepo.SaveChanges();
                    return RedirectToAction("index", "Home", new { Message = "File deleted successfully." });
                }
                else
                {
                    return RedirectToAction("index", "Home", new { Message = "You don't have permission to delete this file." });
                }
                

            }
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        [PasswordConfirmationAttribute]
        public IActionResult DeleteConfirmation(int id, string FileName)
        {
            ViewBag.id = id;
            ViewBag.FileName = FileName;
            return View("Confirmation");
        }

        




        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Search(string search, string? filter)
        {
            if (search == null || search == string.Empty)
            {
                return PartialView("_DocsListPartial", _DocsRepo.GetAll());
            }
            else
            {
                return PartialView("_DocsListPartial", _DocsRepo.Search(search));
            }


        }

        [HttpGet]
        [Authorize]
        //DN = Document Name, VR = Version, CA = Created At, UA = Updated At, SS = Status, UP = Updated By, DD = Downloaded BY
        public IActionResult GSearch(string search, string? DN, string? VR, string? CA, string? UA, string? SS, string? UP, string? DD)
        {
            string[]? SSArray = SS?.Split(',');
            var DocumentInDb = _DocsRepo.Search(search, DN, VR, CA, UA, SSArray);
            var documentIds = DocumentInDb.Select(doc => doc.Id).ToList();
            var currentUserName = PrimacyUser.GetCurrentUserName(_signInManager, User.Identity.Name ?? "").Result;

            //UP show updated by me only from the history actions
            if (UP != null && UP == "on")
            {
                var HistoryActions = _HistoryActionRepo.GetWhere(x => (x.Action == HistoryAction.Updated && x.UserName == currentUserName), x => x.historyLog).ToList();
                var HistoryLogs2 = HistoryActions.Select(x => x.historyLog).ToList() ?? new List<HistoryLog>();
                var documentIds2 = HistoryLogs2.Select(log => log.Document_id.Id).ToList() ?? new List<int>();
                DocumentInDb = DocumentInDb.Where(doc => documentIds2.Contains(doc.Id)).ToList();
            }
            //DD show downloaded by me only from the history actions
            if (DD != null && DD == "on")
            {
                var HistoryActions = _HistoryActionRepo.GetWhere(x => (x.Action == HistoryAction.Downloaded && x.UserName == currentUserName), x => x.historyLog).ToList();
                var HistoryLogs2 = HistoryActions.Select(x => x.historyLog).ToList() ?? new List<HistoryLog>();
                var documentIds2 = HistoryLogs2.Select(log => log.Document_id.Id).ToList() ?? new List<int>();
                DocumentInDb = DocumentInDb.Where(doc => documentIds2.Contains(doc.Id)).ToList();
            }
            return View("InProcess", DocumentInDb);

            
        }
        //[HttpPost]
        //[Authorize(Roles = "Finalizer")]
        //public IActionResult ConfirmApprove(int id, string Filename)
        //{
        //    return PartialView("_DigitalSigniturePopup", _DocsRepo.Find([id, Filename]));
        //}
        [HttpPost]
        [Authorize(Roles = "Finalizer")]
        [PasswordConfirmationAttribute]
        public IActionResult Approve(int id, string Filename)
        {

            MessageResult result = new MessageResult("File not Approved.");
            PrimacyDocument Doc = _DocsRepo.Find([id, Filename]);
            if (Doc.status == PrimacyDocument.Status.Under_Finalization && User.IsInRole("Finalizer"))
            {


                //before approving and converting to pdf
                AuditLogHelper.AddLogThenProcced(HistoryAction.Approved, Doc, _HistoryLogRepo, _HistoryActionRepo, PrimacyUser.GetCurrentUserName(_signInManager, User.Identity.Name ?? "").Result);
                WordDocumentHelper wordDocumenthelper = new WordDocumentHelper(Doc);
                wordDocumenthelper.StampDocument(_HistoryActionRepo, _HistoryLogRepo);
                //wordDocumenthelper.AddWatermark();



                Doc.Approve(_ArchivedDocumentRepo);
                _DocsRepo.SaveChanges();
                result.Status = true;
                result.Message = "File Approved successfully.";

                return RedirectToAction("SendMail", "Mail", new { Filename = Filename, actionTaken = "Approved", status = Doc.status });
            }

            return RedirectToAction("index", "Home", new { Message = result.Status });

        }

        [HttpPost]
        [Authorize(Roles = "Revisor")]
        [PasswordConfirmationAttribute]
        public IActionResult Revise(int id, string Filename)
        {
            MessageResult result = new MessageResult("File not Revised.");
            PrimacyDocument Doc = _DocsRepo.Find([id, Filename]);
            if (Doc.status == PrimacyDocument.Status.Under_Revison && User.IsInRole("Revisor"))
            {

                AuditLogHelper.AddLogThenProcced(HistoryAction.Revised, Doc, _HistoryLogRepo, _HistoryActionRepo, PrimacyUser.GetCurrentUserName(_signInManager, User.Identity.Name!).Result);
                Doc.Revise();
                _DocsRepo.SaveChanges();
                result.Status = true;
                result.Message = "File Revised successfully.";

                return RedirectToAction("SendMail", "Mail", new { Filename = Filename, actionTaken = "Approved", status = Doc.status });
            }

            return RedirectToAction("index", "Home", new { Message = result.Status });

        }

        [HttpPost]
        [Authorize(Roles = "Finalizer ,Revisor")]
        [ValidateAntiForgeryToken]
        public IActionResult RejectPopup(int id, string Filename)
        {
            return PartialView("_ReasonPopup", _DocsRepo.Find([id, Filename]));
        }

        [HttpPost]
        [Authorize(Roles = "Revisor ,Finalizer")]
        [ValidateAntiForgeryToken]
        [PasswordConfirmationAttribute]
        public IActionResult Reject(int id, string Filename, string reason, IFormFile FileWithRejectionComments)
        {
            MessageResult result = new MessageResult("File not Rejected.");
            PrimacyDocument Doc = _DocsRepo.Find([id, Filename]);
            if ((Doc.status == PrimacyDocument.Status.Under_Finalization && User.IsInRole("Finalizer")) || (Doc.status == PrimacyDocument.Status.Under_Revison && User.IsInRole("Revisor")))
            {
                if (FileWithRejectionComments != null)
                {
                    //update the document with the rejection comments
                    MessageResult RejectioResult= Doc.Reject(reason, FileWithRejectionComments);
                    if (!RejectioResult.Status)
                    {
                        result.Status = false;
                        result.Message = RejectioResult.Message;
                        return RedirectToAction("Index", "Home", new {  actionTaken = "Rejected", message = RejectioResult.Message });
                    }
                    else
                    {
                        _DocsRepo.SaveChanges();
                        MessageResult RejectioResult2= _DocsRepo.Update(id, FileWithRejectionComments.FileName);
                        if (!RejectioResult2.Status)
                        {
                            result.Status = false;
                            result.Message = RejectioResult2.Message;
                            return RedirectToAction("Index", "Home", new { actionTaken = "Rejected", message = RejectioResult2.Message });
                        }
                        _DocsRepo.SaveChanges();
                        result.Status = true;
                        result.Message = "File Rejected successfully, Commented File Saved.";
                        AuditLogHelper.AddLogThenProcced(HistoryAction.Rejected_With_Comments, Doc, _HistoryLogRepo, _HistoryActionRepo, PrimacyUser.GetCurrentUserName(_signInManager, User.Identity.Name!).Result);
                        return RedirectToAction("SendMail", "Mail", new { Filename = Filename, actionTaken = "Rejected", status = Doc.status, reason = reason });

                    }
                }
                Doc.Reject(reason);
                _DocsRepo.SaveChanges();
                result.Status = true;
                result.Message = "File Rejected successfully.";
                AuditLogHelper.AddLogThenProcced(HistoryAction.Rejected, Doc, _HistoryLogRepo, _HistoryActionRepo, PrimacyUser.GetCurrentUserName(_signInManager, User.Identity.Name!).Result);
                return RedirectToAction("SendMail", "Mail", new { Filename = Filename, actionTaken = "Rejected", status = Doc.status, reason = reason });
            }

            return RedirectToAction("index", "Home", new { Message = result.Status });
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }

        public static IEnumerable<T> OrderByProperty<T>(IEnumerable<T> items, string propertyName, bool ascending = true)
        {
            PropertyInfo propInfo = typeof(T).GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (propInfo == null)
            {
                throw new ArgumentException($"No property '{propertyName}' found on type '{typeof(T).Name}'");
            }

            return ascending
                ? items.OrderBy(item => propInfo.GetValue(item, null))
                : items.OrderByDescending(item => propInfo.GetValue(item, null));
        }

        public IActionResult PlatformDecide(string Destination)
        {
            if (Destination != "Register")
            {
                Destination = "Login";
            }
            return PartialView("_PlatformDecide", Destination);
        }

        public IActionResult ViewHTML(int id, string Filename)
        {



            //var Doc = _DocsRepo.Find([id, Filename]);

            //XElement html;

            //using (MemoryStream memoryStream = new MemoryStream())
            //{
            //    memoryStream.Write(Doc.Content, 0, Doc.Content.Length);
            //    memoryStream.Position = 0;
            //    using (WordprocessingDocument doc = WordprocessingDocument.Open(memoryStream, true))
            //    {
            //        HtmlConverterSettings settings = new HtmlConverterSettings()
            //        {
            //            PageTitle = "My Page Title",
            //            CssClassPrefix = "htmlTest",
            //            AdditionalCss = "body{width:600px;}"


            //    };
            //        html = HtmlConverter.ConvertToHtml(doc, settings);
            //    }
            //}




            var modelT = new Tuple<int, string>(id, Filename);
            return View("ViewHTML", modelT);
            

        }

        public IActionResult CommentsEditor(int id, string Filename)
        {

            var modelT = new Tuple<int, string>(id, Filename);
            return View("CommentsEditor", modelT);

        }

        public IActionResult GetPdf(int id = 3007, string Filename = "superuser-479118(1)")
        {
            var Doc = _DocsRepo.Find([id, Filename]);
            if (FileTypes.IsFileTypeWord(Doc.FileExtensiton))
            {
                byte[] pdfContent = WordDocumentHelper.ConvertToPdfAndReturnOutput(Doc); // Implement this method to get your PDF content as byte array
                return File(pdfContent, "application/pdf");
            }
            else if(FileTypes.IsFileTypePdf(Doc.FileExtensiton))
            {
                //convert to byte and return content
                return File(Doc.Content, "application/pdf");
            }
            else
            {
                //return an empty byte
                return File(new byte[]{}, "application/pdf");
            }


        }
        public IActionResult GetFile(string Filename)
        {
           
            byte[] pdfContent = System.IO.File.ReadAllBytes("TempView/"+Filename); // Implement this method to get your PDF content as byte array
            return File(pdfContent, "application/pdf");
        }

    }


}
