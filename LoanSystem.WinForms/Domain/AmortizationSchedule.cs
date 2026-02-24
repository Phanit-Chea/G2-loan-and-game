namespace LoanSystem.WinForms.Domain
{
    public class AmortizationSchedule
    {
        public int Year { get; set; }
        public string DateRange { get; set; } = string.Empty;
        public decimal Interest { get; set; }
        public decimal Principal { get; set; }
        public decimal EndingBalance { get; set; }
        public decimal TotalPayment { get; set; }
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
