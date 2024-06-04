using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DoucmentManagmentSys.Models
{
    public class PrimacyUser : IdentityUser
    {

        public string Name { get; set; }
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
