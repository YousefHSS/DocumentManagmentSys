using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Security.Claims;

namespace DoucmentManagmentSys.Models
{
    public class PrimacyUser : IdentityUser
    {
        [Column("Name")]
        [Required(ErrorMessage = "Username is required")]
        public string Name  { get; set; }
        
        [Required(ErrorMessage = "Surname is required")]
        public string Surname { get; set; }
        public string FullName => $"{Name} {Surname}";

        public static async Task<string> GetCurrentUserName(SignInManager<PrimacyUser> userManager, string? email)
        {


            var user = await userManager.UserManager.FindByEmailAsync(email);
            if (user != null)
            {
                return user.FullName;
            }

            return "User not found";
        }
    }

}
