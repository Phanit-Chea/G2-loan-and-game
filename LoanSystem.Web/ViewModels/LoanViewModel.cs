using LoanSystem.Core.Models;
using System.ComponentModel.DataAnnotations;

namespace LoanSystem.Web.ViewModels
{
    public class LoanViewModel
    {
        [Required]
        [Display(Name = "Loan Amount")]
        public decimal Amount { get; set; } = 10000;

        [Required]
        [Display(Name = "Interest Rate (%)")]
        public double Rate { get; set; } = 6.0;

        [Required]
        [Display(Name = "Loan Term (Years)")]
        public int TermYears { get; set; } = 5;

        [Display(Name = "Loan Term (Months)")]
        public int TermMonths { get; set; } = 0;


        [Required]
        [DataType(DataType.Date)]
        [Display(Name = "Start Date")]
        public DateTime StartDate { get; set; } = DateTime.Now;

        // Extra Payments
        [Display(Name = "Extra monthly pay")]
        public decimal ExtraMonthlyPayment { get; set; } = 0;

        [Display(Name = "Extra yearly pay")]
        public decimal ExtraYearlyPayment { get; set; } = 0;

        [Display(Name = "Extra one-time pay")]
        public decimal ExtraOneTimePayment { get; set; } = 0;

        [DataType(DataType.Date)]
        [Display(Name = "One-time payment date")]
        public DateTime? ExtraOneTimePaymentDate { get; set; }

        // Results
        public decimal MonthlyPayment { get; set; }
        public decimal TotalPayment { get; set; }
        public decimal TotalInterest { get; set; }
        public DateTime PayoffDate { get; set; }
        
        public List<AmortizationSchedule>? YearlySchedule { get; set; }
        public List<MonthlyAmortizationSchedule>? MonthlySchedule { get; set; }
    }
}
