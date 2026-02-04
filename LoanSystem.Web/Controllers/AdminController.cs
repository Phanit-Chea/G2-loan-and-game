using LoanSystem.Core.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using LoanSystem.Web.ViewModels;

namespace LoanSystem.Web.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public AdminController(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        public async Task<IActionResult> Dashboard()
        {
            var users = await _userManager.Users.ToListAsync();
            // Filter out the current admin if desired, or show all
            var model = new AdminDashboardViewModel
            {
                Users = users,
                Games = new List<string> { "Tic-Tac-Toe", "Car Racing", "Loan Management System" }
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> ApproveUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.AccountStatus = "Active";
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Dashboard");
        }

        [HttpPost]
        public async Task<IActionResult> RejectUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user != null)
            {
                user.AccountStatus = "Rejected";
                await _userManager.UpdateAsync(user);
            }
            return RedirectToAction("Dashboard");
        }
    }
}
