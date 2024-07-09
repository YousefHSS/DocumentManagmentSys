
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace DoucmentManagmentSys.Models
{

    public class HistoryLog
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int id { get; set; }
        public required PrimacyDocument Document_id { get; set; }
        public ICollection<HistoryAction> HistoryActions { get; set; } = new List<HistoryAction>();


    }
}
