using LoanSystem.Core.Models;
using LoanSystem.Web.Data;
using LoanSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace LoanSystem.Web.Controllers
{
    [Authorize(Roles = "User")]
    public class UserController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public UserController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public async Task<IActionResult> Dashboard()
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return NotFound();

            var history = await _context.GameHistories
                .Where(h => h.UserId == user.Id)
                .OrderByDescending(h => h.PlayedAt)
                .Take(20)
                .ToListAsync();

            var loanHistory = await _context.LoanHistories
                .Where(h => h.UserId == user.Id)
                .OrderByDescending(h => h.CreatedAt)
                .Take(20)
                .ToListAsync();

            var model = new UserDashboardViewModel
            {
                User = user,
                GameHistory = history,
                LoanHistory = loanHistory
            };

            return View(model);
        }
    }
}
