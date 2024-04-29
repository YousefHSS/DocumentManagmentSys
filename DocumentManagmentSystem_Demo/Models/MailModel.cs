using Microsoft.AspNetCore.Mvc.ModelBinding.Binders;

namespace DocumentManagmentSystem_Demo.Models
{
    public class MailModel
    {
        public string link;
        public string action;
        public string? Reason { get; set; }
        public MailModel(string link, string action, string? reason)
        {
            this.link = link;
            this.action = action;
            Reason = reason;
        }



    }
}
