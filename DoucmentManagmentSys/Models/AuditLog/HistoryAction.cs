//using DoucmentManagmentSys.Migrations;
using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoucmentManagmentSys.Models
{
    public class HistoryAction
    {
        [Key]
        public int id { get; set; }
        public required string Action { get; set; }

        [ForeignKey("AspNetUsers")]
        public required string UserName { get; set; }

        [Required]
        public HistoryLog historyLog { get; set; }
        public DateTime CreatedAt { get; set; }
        public HistoryAction()
        {
            CreatedAt = DateTime.Now;
        }
        public HistoryAction(string action, string userName)
        {
            Action = action;
            UserName = userName;
            CreatedAt = DateTime.Now;
        }
        public static string Created = "Prepared";
        public static string Updated = "Updated";
        public static string Deleted = "Deleted";
        public static string Approved = "Approved";
        public static string Rejected = "Rejected";
        public static string Revised = "Checked";
        public static string Downloaded = "Downloaded";

        internal static string[] GetAllActionTypes()
        {
            return [Created, Updated, Deleted, Approved, Rejected, Revised, Downloaded];
        }
    }
}
