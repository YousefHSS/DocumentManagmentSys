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
            //return partial view
            return View(model: HistoryActions);
        }
        [HttpPost]
        public  void  AddLogThenProcced(string actionName,int id, string doc_name) 
        {
            
            ////get username from session
            //var test = User.Identity.Name;

            ////create history action
            //HistoryAction historyAction = new HistoryAction {
            //    Action = actionName,
            //    UserName = test?? "not Found"
            //};
            ////get document by id
            //var Document = _DocumentRepo.Find([id, doc_name]);
            ////check history log of document if exist
            //var historyLog = _HistoryLogRepo.GetWhere(x => x.Document_id == Document).FirstOrDefault();
            //_HistoryActionRepo.Add(historyAction);
            //if (historyLog == null)
            //{

            //    //create new history log
            //    HistoryLog New_historyLog = new HistoryLog
            //    {
            //        Document_id = Document,
            //        HistoryActions = new Collection<HistoryAction> { historyAction }
            //    };
            //    //add history log to db
            //    _HistoryLogRepo.Add(New_historyLog);
            //}
            //else
            //{

            //    //add history action to history log
            //    historyLog.HistoryActions.Add(historyAction);
                
            //    //update history log
            //    _HistoryLogRepo.Update(historyLog);
            //}
            //_HistoryLogRepo.SaveChanges();
            //_HistoryActionRepo.SaveChanges();
            ViewBag.message = "done";
        }



    }
}
