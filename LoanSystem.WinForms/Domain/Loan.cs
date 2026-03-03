namespace LoanSystem.WinForms.Domain
{
    public class Loan : LoanBase
    {
        public override decimal LoanAmount { get; set; }
        public override double InterestRate { get; set; }
        public override int TermYears { get; set; }
        public override int TermMonths { get; set; }
        public override DateTime StartDate { get; set; }
        public decimal ExtraMonthlyPayment { get; set; }
        public decimal ExtraYearlyPayment { get; set; }
        public decimal ExtraOneTimePayment { get; set; }
        public DateTime? ExtraOneTimePaymentDate { get; set; }

        public override decimal CalculateMonthlyPayment()
        {
            var monthlyRate = MonthlyInterestRate;
            var totalMonths = TotalMonths;

            if (totalMonths <= 0)
            {
                return 0;
            }

            if (monthlyRate == 0)
            {
                return Math.Round(LoanAmount / totalMonths, 2);
            }

            var payment = (double)LoanAmount *
                          (monthlyRate * Math.Pow(1 + monthlyRate, totalMonths)) /
                          (Math.Pow(1 + monthlyRate, totalMonths) - 1);

            return Math.Round((decimal)payment, 2);
        }

        public override List<MonthlyAmortizationSchedule> GenerateMonthlySchedule()
        {
            var schedule = new List<MonthlyAmortizationSchedule>();
            var balance = LoanAmount;
            var monthlyPayment = CalculateMonthlyPayment();
            var monthlyRate = MonthlyInterestRate;
            var totalMonths = TotalMonths;
            var currentDate = StartDate;

            for (var i = 1; i <= totalMonths; i++)
            {
                if (balance <= 0)
                {
                    break;
                }

                var interest = Math.Round(balance * (decimal)monthlyRate, 2);
                var principal = monthlyPayment - interest;
                var extra = ExtraMonthlyPayment;

                if (i % 12 == 0)
                {
                    extra += ExtraYearlyPayment;
                }

                if (ExtraOneTimePaymentDate.HasValue &&
                    currentDate.Month == ExtraOneTimePaymentDate.Value.Month &&
                    currentDate.Year == ExtraOneTimePaymentDate.Value.Year)
                {
                    extra += ExtraOneTimePayment;
                }

                principal += extra;

                if (balance < principal)
                {
                    principal = balance;
                    monthlyPayment = principal + interest;
                }

                balance -= principal;

                schedule.Add(new MonthlyAmortizationSchedule
                {
                    Month = i,
                    Date = currentDate,
                    Payment = monthlyPayment + (balance == 0 ? 0 : extra),
                    Principal = principal,
                    Interest = interest,
                    Balance = balance
                });

                currentDate = currentDate.AddMonths(1);
            }

            return schedule;
        }

        public override List<AmortizationSchedule> GenerateYearlySchedule()
        {
            var monthlySchedule = GenerateMonthlySchedule();
            var yearlySchedule = new List<AmortizationSchedule>();

            var currentYear = 0;
            decimal yearInterest = 0;
            decimal yearPrincipal = 0;
            decimal endingBalance = 0;
            var yearStart = DateTime.MinValue;
            var yearEnd = DateTime.MinValue;

            foreach (var month in monthlySchedule)
            {
                if (currentYear == 0)
                {
                    currentYear = month.Date.Year;
                }

                if (month.Date.Year != currentYear)
                {
                    yearlySchedule.Add(new AmortizationSchedule
                    {
                        Year = currentYear,
                        DateRange = $"{yearStart:MM/yy}-{yearEnd:MM/yy}",
                        Interest = yearInterest,
                        Principal = yearPrincipal,
                        EndingBalance = endingBalance,
                        TotalPayment = yearInterest + yearPrincipal
                    });

                    currentYear = month.Date.Year;
                    yearInterest = 0;
                    yearPrincipal = 0;
                    yearStart = month.Date;
                }

                if (yearStart == DateTime.MinValue)
                {
                    yearStart = month.Date;
                }

                yearEnd = month.Date;
                yearInterest += month.Interest;
                yearPrincipal += month.Principal;
                endingBalance = month.Balance;
            }

            if (yearInterest > 0 || yearPrincipal > 0)
            {
                yearlySchedule.Add(new AmortizationSchedule
                {
                    Year = currentYear,
                    DateRange = $"{yearStart:MM/yy}-{yearEnd:MM/yy}",
                    Interest = yearInterest,
                    Principal = yearPrincipal,
                    EndingBalance = endingBalance,
                    TotalPayment = yearInterest + yearPrincipal
                });
            }

            var yearCount = 1;
            foreach (var item in yearlySchedule)
            {
                item.Year = yearCount++;
            }

            return yearlySchedule;
        }
    }
}
