namespace LoanSystem.WinForms.Domain
{
    public abstract class LoanBase
    {
        public abstract decimal LoanAmount { get; set; }
        public abstract double InterestRate { get; set; }
        public abstract int TermYears { get; set; }
        public abstract int TermMonths { get; set; }
        public abstract DateTime StartDate { get; set; }

        protected int TotalMonths => (TermYears * 12) + TermMonths;
        protected double MonthlyInterestRate => (InterestRate / 100d) / 12d;

        public abstract decimal CalculateMonthlyPayment();
        public abstract List<MonthlyAmortizationSchedule> GenerateMonthlySchedule();
        public abstract List<AmortizationSchedule> GenerateYearlySchedule();
    }
}
