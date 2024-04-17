using DoucmentManagmentSys.Models;
using DoucmentManagmentSys.Repo;
using System.Collections.ObjectModel;


namespace DoucmentManagmentSys.Controllers.Helpers
{
    public class AuditLogHelper
    {

        public AuditLogHelper() { }
        public static void AddLogThenProcced(string actionName,PrimacyDocument document , MainRepo<HistoryLog> _HistoryLogRepo,  MainRepo<HistoryAction> _HistoryActionRepo, string Username)
        {
            

            //create history action
            HistoryAction historyAction = new HistoryAction
            {
                Action = actionName,
                UserName = Username ?? "not Found"
            };

            //check history log of document if exist
            var historyLog = _HistoryLogRepo.GetWhere(x => x.Document_id == document).FirstOrDefault();
            _HistoryActionRepo.Add(historyAction);
            if (historyLog == null)
            {

                //create new history log
                HistoryLog New_historyLog = new HistoryLog
                {
                    Document_id = document,
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

        }
        public static void AddLogThenProcced(string actionName, string docname,int id, DocumentRepository _DocsRepo, MainRepo<HistoryLog> _HistoryLogRepo, MainRepo<HistoryAction> _HistoryActionRepo,  string Username)
        {
            var document = _DocsRepo.Find([id, docname]);
            //create history action
            HistoryAction historyAction = new HistoryAction
            {
                Action = actionName,
                UserName = Username ?? "not Found"
            };

            //check history log of document if exist
            var historyLog = _HistoryLogRepo.GetWhere(x => x.Document_id == document).FirstOrDefault();
            _HistoryActionRepo.Add(historyAction);
            if (historyLog == null)
            {

                //create new history log
                HistoryLog New_historyLog = new HistoryLog
                {
                    Document_id = document,
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

        }

        internal static List<HistoryAction> GetLatestActionsOfDocument(PrimacyDocument doc, MainRepo<HistoryAction> _HistoryActionRepo, MainRepo<HistoryLog> _HistoryLogRepo)
        {
            //get all action Types
            var actionTypes = HistoryAction.GetAllActionTypes();
            //get history log of doc
            var historyLog = _HistoryLogRepo.GetWhere(x => x.Document_id == doc).FirstOrDefault();
            //get latest history action of each action type for this doc
            List<HistoryAction> latestActions = new List<HistoryAction>();

            foreach (var actionType in actionTypes)
            {
                // Get the latest history action of each action type for this doc
                var latestAction = _HistoryActionRepo.GetWhere(x => x.Action == actionType && x.historyLog == historyLog)
                                                     .OrderByDescending(x => x.CreatedAt) // Assuming there is a Date field to sort by
                                                     .FirstOrDefault();
                if (latestAction != null)
                {
                    latestActions.Add(latestAction);
                }
            }

            return latestActions;


        }
    }
}
