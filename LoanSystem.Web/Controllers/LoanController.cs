using LoanSystem.Core.Models;
using LoanSystem.Web.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using LoanSystem.Web.Data;
using Microsoft.AspNetCore.Identity;

namespace LoanSystem.Web.Controllers
{
    [Authorize]
    public class LoanController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<ApplicationUser> _userManager;

        public LoanController(ApplicationDbContext context, UserManager<ApplicationUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }
        [HttpGet]
        public IActionResult Index()
        {
            return View(new LoanViewModel());
        }

        [HttpPost]
        public IActionResult Calculate(LoanViewModel model)
        {
            if (ModelState.IsValid)
            {
                var loan = new Loan
                {
                    LoanAmount = model.Amount,
                    InterestRate = model.Rate,
                    TermYears = model.TermYears,
                    TermMonths = model.TermMonths,
                    StartDate = model.StartDate,
                    ExtraMonthlyPayment = model.ExtraMonthlyPayment,
                    ExtraYearlyPayment = model.ExtraYearlyPayment,
                    ExtraOneTimePayment = model.ExtraOneTimePayment,
                    ExtraOneTimePaymentDate = model.ExtraOneTimePaymentDate
                };

                model.MonthlyPayment = loan.CalculateMonthlyPayment();
                model.MonthlySchedule = loan.GenerateMonthlySchedule();
                model.YearlySchedule = loan.GenerateYearlySchedule();
                
                if (model.MonthlySchedule.Any())
                {
                    model.TotalPayment = model.MonthlySchedule.Sum(x => x.Payment);
                    model.TotalInterest = model.MonthlySchedule.Sum(x => x.Interest);
                    model.PayoffDate = model.MonthlySchedule.Last().Date;

                    // Save History
                    var userId = _userManager.GetUserId(User);
                    if(userId != null)
                    {
                        var history = new LoanHistory
                        {
                            UserId = userId,
                            Amount = model.Amount,
                            InterestRate = model.Rate,
                            TermYears = model.TermYears,
                            TermMonths = model.TermMonths,
                            TotalPayment = model.TotalPayment,
                            CreatedAt = DateTime.Now
                        };
                        _context.LoanHistories.Add(history);
                        _context.SaveChanges();
                    }
                }
                
                return View("Index", model);
            }
            return View("Index", model);
        }

        [HttpPost]
        public IActionResult Export(LoanViewModel model)
        {
             if (ModelState.IsValid)
            {
                var loan = new Loan
                {
                    LoanAmount = model.Amount,
                    InterestRate = model.Rate,
                    TermYears = model.TermYears,
                    TermMonths = model.TermMonths,
                    StartDate = model.StartDate
                };

                var schedule = loan.GenerateMonthlySchedule();
                var csv = "Month,Date,Payment,Principal,Interest,Balance\n";

                foreach (var item in schedule)
                {
                    csv += $"{item.Month},{item.Date:yyyy-MM-dd},{item.Payment},{item.Principal},{item.Interest},{item.Balance}\n";
                }

                return File(System.Text.Encoding.UTF8.GetBytes(csv), "text/csv", "AmortizationSchedule.csv");
            }
            return RedirectToAction("Index");
        }
    }
}
