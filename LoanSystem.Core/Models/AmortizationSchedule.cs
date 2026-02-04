using System;

namespace LoanSystem.Core.Models
{
    public class AmortizationSchedule
    {
        public int Year { get; set; }
        public string DateRange { get; set; } // e.g., "1/26-12/26"
        public decimal Interest { get; set; }
        public decimal Principal { get; set; }
        public decimal EndingBalance { get; set; }
        public decimal TotalPayment { get; set; } // Monthly Payment * 12 or remaining
    }

    public class MonthlyAmortizationSchedule
    {
        public int Month { get; set; }
        public DateTime Date { get; set; }
        public decimal Payment { get; set; }
        public decimal Principal { get; set; }
        public decimal Interest { get; set; }
        public decimal Balance { get; set; }
    }
}
