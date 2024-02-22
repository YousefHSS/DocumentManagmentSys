using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace DoucmentManagmentSys.Models
{
    public class MailModel
    {
        public string link;
        public string action;

        public MailModel(string link, string action)
        {
            this.link = link;
            this.action = action;
        }



    }
}
