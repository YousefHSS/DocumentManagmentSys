using DocumentManagmentSystem_Demo.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace DocumentManagmentSystem_Demo.RoleManagment
{
    public interface IRoleManagment
    {
        UserManager<PrimacyUser> userManager { get; set; }
        RoleManager<IdentityRole> roleManager { get; set; }




        public Task<bool> CreateRoles(IServiceProvider serviceProvider);

        public Task<bool> SwitchRole(ClaimsPrincipal User, string role);

        public Task<bool> SwitchRole(PrimacyUser user, string role);

        public Task<bool> CheckRole(ClaimsIdentity User, string role);

        public string[] GetUsersByRole(string role);
    }
}
