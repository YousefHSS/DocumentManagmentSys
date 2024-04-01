using System.Collections.ObjectModel;
using DoucmentManagmentSys.Models;
using DoucmentManagmentSys.Repo;
using Microsoft.AspNet.Identity;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace DoucmentManagmentSys.Controllers
{
    
    public class HistoryLogController : Controller
    {
        public SignInManager<IdentityUser> _signInManager { get; set; }
        private readonly MainRepo<HistoryAction> _HistoryActionRepo;
        private readonly MainRepo<HistoryLog> _HistoryLogRepo;
        private readonly DocumentRepository _DocumentRepo;


        public HistoryLogController( SignInManager<IdentityUser> signInManager, MainRepo<HistoryAction> repository, MainRepo<HistoryLog> repositorylog , DocumentRepository repositorydoc)
        {
            _signInManager = signInManager;
            _HistoryActionRepo = repository;
            _HistoryLogRepo = repositorylog;
            _DocumentRepo = repositorydoc;
        }

        public IActionResult Index()
        {
            //return partial view
            return PartialView();
        }
        [HttpGet]
        public  IActionResult  AddActionToLog(string action,int id, string doc_name) 
        {

            //get username from session
            var test = GetNormalizedUserName(User.Identity.GetUserId()).Result;

            //create history action
            HistoryAction historyAction = new HistoryAction {
                Action = action,
                UserName = test?? "not Found"
            };
            //get document by id
            var Document = _DocumentRepo.Find([id, doc_name]);
            //check history log of document if exist
            var historyLog = _HistoryLogRepo.GetWhere(x => x.Document_id == Document).FirstOrDefault();
            _HistoryActionRepo.Add(historyAction);
            if (historyLog == null)
            {

                //create new history log
                HistoryLog New_historyLog = new HistoryLog
                {
                    Document_id = Document,
                    HistoryActions = new Collection<HistoryAction> { historyAction }
                };
                //add history log to db
                _HistoryLogRepo.Add(New_historyLog);
            }
            else
            {

                //add history action to history log
                historyLog.HistoryActions.Add(historyAction);
                
                //update history log
                _HistoryLogRepo.Update(historyLog);
            }
            _HistoryLogRepo.SaveChanges();
            _HistoryActionRepo.SaveChanges();
            return RedirectToAction("index", "Home", new { Message = "Action Added Successfully" });






        }
        public async Task<string?> GetNormalizedUserName(string userId)
        {
            // Retrieve the user by their ID
            var user = await _signInManager.UserManager.FindByIdAsync(userId);

            if (user == null)
            {
                return null; // Or handle the scenario where the user is not found
            }

            // Get the normalized username
            var normalizedUserName = user.NormalizedUserName;

            return normalizedUserName;
        }


    }
}
