using DoucmentManagmentSys.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoucmentManagmentSys.Data
{
    public class ApplicationDbContext : IdentityDbContext<PrimacyUser>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }



        DbSet<PrimacyDocument> Documents { get; set; }
        DbSet<HistoryLog> HistoryLogs { get; set; }
        DbSet<HistoryAction> HistoryActions { get; set; }
    }
}
