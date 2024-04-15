using System.Collections.ObjectModel;
using DoucmentManagmentSys.Models;
using DoucmentManagmentSys.Repo;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DoucmentManagmentSys.Controllers
{

    public class HistoryLogController : Controller
    {
        private readonly MainRepo<HistoryAction> _HistoryActionRepo;
        private readonly MainRepo<HistoryLog> _HistoryLogRepo;
        private readonly DocumentRepository _DocumentRepo;


        public HistoryLogController(MainRepo<HistoryAction> repository, MainRepo<HistoryLog> repositorylog, DocumentRepository repositorydoc)
        {
            _HistoryActionRepo = repository;
            _HistoryLogRepo = repositorylog;
            _DocumentRepo = repositorydoc;
        }

        [Authorize]
        public IActionResult Index(string doc_name)
        {
                        //get document by name
            var Document = _DocumentRepo.GetWhere(x => x.FileName == doc_name).FirstOrDefault();

            //get history log of document
            var historyLog = _HistoryLogRepo.GetWhere(x => x.Document_id == Document).FirstOrDefault();
            //return view with history log
            //get all history actions with that log id
            List<HistoryAction>? HistoryActions =null;
            if (historyLog != null)
            {
                HistoryActions = _HistoryActionRepo.GetWhere(x => x.historyLog == historyLog).ToList();
            }
            TempData["Document"] = doc_name;
            ViewData["Title"] = "Audit Log";
            //return partial view
            return View(model: HistoryActions);
        }
       



    }
}
