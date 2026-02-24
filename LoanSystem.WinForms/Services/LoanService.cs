using System.Text;
using LoanSystem.WinForms.Data;
using LoanSystem.WinForms.Domain;

namespace LoanSystem.WinForms.Services
{
    public class LoanService
    {
        private readonly IAppDataStore _store;

        public LoanService(IAppDataStore store)
        {
            _store = store;
        }

        public (decimal MonthlyPayment, decimal TotalPayment, decimal TotalInterest, DateTime PayoffDate, List<MonthlyAmortizationSchedule> MonthlySchedule, List<AmortizationSchedule> YearlySchedule) Calculate(Loan loan)
        {
            var monthlyPayment = loan.CalculateMonthlyPayment();
            var monthlySchedule = loan.GenerateMonthlySchedule();
            var yearlySchedule = loan.GenerateYearlySchedule();

            var totalPayment = monthlySchedule.Sum(x => x.Payment);
            var totalInterest = monthlySchedule.Sum(x => x.Interest);
            var payoffDate = monthlySchedule.Any() ? monthlySchedule.Last().Date : loan.StartDate;

            return (monthlyPayment, totalPayment, totalInterest, payoffDate, monthlySchedule, yearlySchedule);
        }

        public void SaveHistory(string userId, Loan loan, decimal totalPayment)
        {
            var history = new LoanHistory
            {
                UserId = userId,
                Amount = loan.LoanAmount,
                InterestRate = loan.InterestRate,
                TermYears = loan.TermYears,
                TermMonths = loan.TermMonths,
                TotalPayment = totalPayment,
                CreatedAt = DateTime.Now
            };

            _store.AddLoanHistory(history);
        }

        public string BuildCsv(List<MonthlyAmortizationSchedule> schedule)
        {
            var builder = new StringBuilder();
            builder.AppendLine("Month,Date,Payment,Principal,Interest,Balance");
            foreach (var item in schedule)
            {
                builder.AppendLine($"{item.Month},{item.Date:yyyy-MM-dd},{item.Payment},{item.Principal},{item.Interest},{item.Balance}");
            }

            return builder.ToString();
        }
    }
}
