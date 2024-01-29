using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using System.Security.Principal;

namespace DoucmentManagmentSys.RoleManagment
{
    public class RoleManagment : IRoleManagment
    {

        public required UserManager<IdentityUser> userManager { get; set; }
        public required RoleManager<IdentityRole> roleManager { get; set; }


        public RoleManagment(UserManager<IdentityUser> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.userManager = userManager;
            this.roleManager = roleManager;

        }
        public async Task<bool> AssignRole(ClaimsPrincipal User, string role)
        {


            var user = await userManager.GetUserAsync(User);
            if (user == null)
            {
                throw new Exception("User does not exist.");
            }
            var roleExists = await roleManager.RoleExistsAsync(role);
            if (!roleExists)
            {
                throw new Exception("Role does not exist.");
            }

            var result = await userManager.AddToRoleAsync(user, role);
            var isInRole = await userManager.IsInRoleAsync(user, role);
            if (isInRole)
            {
                //update the user's claims princibal
                var claims = await userManager.GetClaimsAsync(user);
                var claim = claims.FirstOrDefault(c => c.Type == ClaimTypes.Role);
                if (claim != null)
                {
                    await userManager.RemoveClaimAsync(user, claim);
                }
                await userManager.AddClaimAsync(user, new Claim(ClaimTypes.Role, role));




                // User is already in the role, no need to assign it again
                return false;
            }


            if (!result.Succeeded)
            {
                throw new Exception("Failed to assign role to user. " + result.Errors);
            }

            return true;
        }

        public async Task<bool> CheckRole(IdentityUser User, string role)
        {
            var isInRole = await userManager.IsInRoleAsync(User, role);
            return isInRole;
        }

        public Task<bool> CheckRole(ClaimsIdentity User, string role)
        {
            throw new NotImplementedException();
        }

        public async Task<bool> CreateRoles(IServiceProvider serviceProvider)
        {
            var roleNames = new List<string> { "Admin", "User" };

            foreach (var roleName in roleNames)
            {
                var roleExists = await roleManager.RoleExistsAsync(roleName);
                if (!roleExists)
                {
                    var role = new IdentityRole(roleName);
                    var result = await roleManager.CreateAsync(role);
                    if (!result.Succeeded)
                    {
                        throw new Exception($"Failed to create role: {roleName}");
                    }
                }
            }

            return true;
        }

    }
}
