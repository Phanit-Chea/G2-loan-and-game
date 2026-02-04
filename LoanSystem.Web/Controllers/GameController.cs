using LoanSystem.Core.Models;
using LoanSystem.Web.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace LoanSystem.Web.Controllers
{
    [Authorize]
    public class GameController : Controller
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ApplicationDbContext _context;

        public GameController(UserManager<ApplicationUser> userManager, ApplicationDbContext context)
        {
            _userManager = userManager;
            _context = context;
        }

        public IActionResult TicTacToe()
        {
            return View();
        }

        public IActionResult CarRacing()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> SaveScore([FromBody] GameHistory model)
        {
            var user = await _userManager.GetUserAsync(User);
            if (user == null) return Unauthorized();

            model.UserId = user.Id;
            model.PlayedAt = DateTime.Now;

            _context.GameHistories.Add(model);
            await _context.SaveChangesAsync();

            return Ok(new { success = true });
        }
    }
}
