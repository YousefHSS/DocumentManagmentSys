
using DoucmentManagmentSys.RoleManagment;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;


namespace DoucmentManagmentSys.Controllers
{
    // only accessable by admin
    [Authorize(Roles = "Admin")]
    public class DashboardController : Controller
    {


        private readonly UserManager<IdentityUser> _userManager;

        private readonly IRoleManagment _roleManagment;

        public DashboardController(UserManager<IdentityUser> userManager, IRoleManagment roleManagment)
        {
            _userManager = userManager;
            _roleManagment = roleManagment;
        }
        // Action to display the dashboard
        public ActionResult index()
        {
            // Get the list of users from your data source
            // Get the currently authenticated user
            var currentUser = _userManager.GetUserAsync(User).Result;

            // Get the list of all users
            var users = _userManager.Users.ToList();

            ViewData["Title"] = "Dashboard";
            return View(users);
        }

        // Action to handle role changes
        [HttpPost]
        public async Task<bool> ChangeRole(string userId, string newRole)
        {

            // Get the user
            var user = _userManager.FindByIdAsync(userId).Result;
            var Result = false;
            if (user != null)
            {
                Result = await _roleManagment.SwitchRole(user, newRole);
            }
            return Result;

        }

    }
}
