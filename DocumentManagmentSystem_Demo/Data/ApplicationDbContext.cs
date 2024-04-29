using DocumentManagmentSystem_Demo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DocumentManagmentSystem_Demo.Data
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

        DbSet<ArchivedDocument> archivedDocuments { get; set; }

        DbSet<ArchivedVersion> archivedVersions  { get; set; }


    }
}
