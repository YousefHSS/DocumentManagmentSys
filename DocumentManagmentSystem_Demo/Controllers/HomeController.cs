using DoucmentManagmentSys.Models;
using Microsoft.AspNetCore.Mvc;
using System.Diagnostics;
using DoucmentManagmentSys.Repo;
using DoucmentManagmentSys.RoleManagment;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNet.Identity;
using System.Reflection;
using DoucmentManagmentSys.Helpers;
using System.Linq;



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

        public IActionResult Index(string Message, string Messages, string? SortBY)
        {
            ViewBag.Message = Message ?? "";
            ViewBag.Messages = Messages ?? "";
            TempData["Id"] = TempData["Id"] ?? "";

            return View(SortBY != null ? OrderByProperty<PrimacyDocument>(_DocsRepo.GetAll(), SortBY) : _DocsRepo.GetAll());
        }

        [HttpPost]
        [Authorize(Roles = "Uploader,Revisor")]
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
            List<PrimacyDocument> documents = await ServerFileManager.FilesToDocs(User.Identity.GetUserId());
            MessageResult Result = _DocsRepo.AddRange(documents);

            if (Result.Status)
            {
                AuditLogHelper.AddLogThenProcced(HistoryAction.Created, documents[0], _HistoryLogRepo, _HistoryActionRepo, PrimacyUser.GetCurrentUserName(_signInManager, User.Identity.Name!).Result);

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
        public IActionResult DeleteFile(int id, string fileName)
        {
            PrimacyDocument document = _DocsRepo.Find([id, fileName]);
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
        public IActionResult GSearch(string search,string? DN,string? VR, string? CA,string? UA, string? SS,string? UP, string? DD)
        {
            string[]? SSArray = SS?.Split(',');
            var DocumentInDb = _DocsRepo.Search(search, DN, VR, CA, UA, SSArray);
            var documentIds = DocumentInDb.Select(doc => doc.Id).ToList();
            var currentUserName = PrimacyUser.GetCurrentUserName(_signInManager, User.Identity.Name?? "").Result;

            //UP show updated by me only from the history actions
            if (UP != null && UP == "on")
            {
                var HistoryActions = _HistoryActionRepo.GetWhere(x=> (x.Action == HistoryAction.Updated && x.UserName == currentUserName),x=>x.historyLog).ToList();
                var HistoryLogs2 = HistoryActions.Select(x => x.historyLog).ToList() ?? new List<HistoryLog>();
                var documentIds2 = HistoryLogs2.Select(log => log.Document_id.Id).ToList() ?? new List<int>();
                DocumentInDb = DocumentInDb.Where(doc=> documentIds2.Contains(doc.Id)).ToList();
            }
            //DD show downloaded by me only from the history actions
            if (DD != null && DD == "on")
            {
                var HistoryActions = _HistoryActionRepo.GetWhere(x => (x.Action == HistoryAction.Downloaded && x.UserName == currentUserName), x => x.historyLog).ToList();
                var HistoryLogs2 = HistoryActions.Select(x => x.historyLog).ToList() ?? new List<HistoryLog>();
                var documentIds2 = HistoryLogs2.Select(log => log.Document_id.Id).ToList() ?? new List<int>();
                DocumentInDb = DocumentInDb.Where(doc => documentIds2.Contains(doc.Id)).ToList();
            }
            return View("index", DocumentInDb);


        }
        [HttpPost]
        [Authorize(Roles = "Finalizer")]
        public IActionResult ConfirmApprove(int id, string Filename)
        {
            return PartialView("_DigitalSigniturePopup", _DocsRepo.Find([id, Filename]));
        }
        [HttpPost]
        [Authorize(Roles = "Finalizer")]
        public IActionResult Approve(int id, string Filename , string Stamp="off")
        {

            MessageResult result = new MessageResult("File not Approved.");
            PrimacyDocument Doc = _DocsRepo.Find([id, Filename]);
            if (Doc.status == PrimacyDocument.Status.Under_Finalization && User.IsInRole("Finalizer"))
            {


                //before approving and converting to pdf
                AuditLogHelper.AddLogThenProcced(HistoryAction.Approved, Doc, _HistoryLogRepo, _HistoryActionRepo, PrimacyUser.GetCurrentUserName(_signInManager, User.Identity.Name??"").Result);
                WordDocumentHelper wordDocumenthelper = new WordDocumentHelper(Doc);
                if (Stamp=="on")
                {
                    wordDocumenthelper.StampDocument(_HistoryActionRepo, _HistoryLogRepo);
                }
                

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
        public IActionResult Reject(int id, string Filename, string reason)
        {
            MessageResult result = new MessageResult("File not Rejected.");
            PrimacyDocument Doc = _DocsRepo.Find([id, Filename]);
            if ((Doc.status == PrimacyDocument.Status.Under_Finalization && User.IsInRole("Finalizer")) || (Doc.status == PrimacyDocument.Status.Under_Revison && User.IsInRole("Revisor")))
            {
                //displayRejectPopup


                Doc.Reject(reason);
                _DocsRepo.SaveChanges();
                result.Status = true;
                result.Message = "File Rejected successfully.";
                AuditLogHelper.AddLogThenProcced(HistoryAction.Rejected, Doc, _HistoryLogRepo, _HistoryActionRepo, PrimacyUser.GetCurrentUserName(_signInManager, User.Identity.Name!).Result);
                return RedirectToAction("SendMail", "Mail", new { Filename = Filename, actionTaken = "Rejected", status = Doc.status, reason = reason });
            }

            return RedirectToAction("index", "Home", new { Message = result.Status });
        }



        public IActionResult Docs()
        {
            return View();
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
                Destination="Login";
            }
            return PartialView("_PlatformDecide", Destination);
        }

    }

  
}
