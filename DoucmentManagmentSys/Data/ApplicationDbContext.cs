using DoucmentManagmentSys.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace DoucmentManagmentSys.Data
{
    public class ApplicationDbContext : IdentityDbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

           

            //a hasher to hash the password before seeding the user to the db
            var hasher = new PasswordHasher<IdentityUser>();


            //Seeding the User to AspNetUsers table
            modelBuilder.Entity<IdentityUser>().HasData(
                new IdentityUser
                {
                    Id = "8e445865-a24d-4543-a6c6-9443d048cdb9", // primary key
                    UserName = "Uploader",
                    EmailConfirmed = true,
                    Email = "Uploader@email.com",
                    NormalizedUserName = "UPLOADER",
                    PasswordHash = hasher.HashPassword(null, "Pa$$w0rd")
                }
            );
            
            //Revisor Account
            modelBuilder.Entity<IdentityUser>().HasData(
                new IdentityUser
                {
                    Id = "3b62472e-4f66-49fa-a20f-e7685b9565d9", // primary key
                    UserName = "Revisor",
                    EmailConfirmed = true,
                    Email = "Revisor@email.com",
                    NormalizedUserName = "REVISOR",
                    PasswordHash = hasher.HashPassword(null, "Pa$$w0rd")
                }
            );

            //Finalizer Account

            modelBuilder.Entity<IdentityUser>().HasData(
                new IdentityUser
                {
                    Id = "3b62472e-4f66-49fa-a20f-e7685b9565d6", // primary key
                    UserName = "Finalizer",
                    EmailConfirmed = true,
                    Email = "Finalizer@email.com",
                    NormalizedUserName = "FINALIZER",
                    PasswordHash = hasher.HashPassword(null, "Pa$$w0rd")
                }
            );

            //create Roles
            modelBuilder.Entity<IdentityRole>().HasData(
                new IdentityRole
                {
                    Id = "cac43a6e-f7bb-4448-baaf-1add431ccbbf",
                    Name = "Uploader",
                    NormalizedName = "UPLOADER"
                },
                new IdentityRole
                {
                    Id = "cac43a6e-f7bb-4448-baaf-1add431ccbbd",
                    Name = "Revisor",
                    NormalizedName = "REVISOR"
                },
                new IdentityRole
                {
                    Id = "cac43a6e-f7bb-4448-baaf-1add431ccbbe",
                    Name = "Finalizer",
                    NormalizedName = "FINALIZER"
                }

            );

                        //assign roles to users
            modelBuilder.Entity<IdentityUserRole<string>>().HasData(
                new IdentityUserRole<string>
                {
                    RoleId = "cac43a6e-f7bb-4448-baaf-1add431ccbbf",
                    UserId = "8e445865-a24d-4543-a6c6-9443d048cdb9"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "cac43a6e-f7bb-4448-baaf-1add431ccbbd",
                    UserId = "3b62472e-4f66-49fa-a20f-e7685b9565d9"
                },
                new IdentityUserRole<string>
                {
                    RoleId = "cac43a6e-f7bb-4448-baaf-1add431ccbbe",
                    UserId = "3b62472e-4f66-49fa-a20f-e7685b9565d6"
                }

            );




        }

        DbSet<PrimacyDocument> Documents { get; set; }
        DbSet<HistoryLog> HistoryLogs { get; set; }
        DbSet<HistoryAction> HistoryActions { get; set; }
    }
}
