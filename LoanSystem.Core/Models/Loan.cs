using System;
using System.Collections.Generic;
using System.Security.Cryptography.X509Certificates;

namespace LoanSystem.Core.Models
{
    public class Loan
    {
        public decimal LoanAmount { get; set; }
        public double InterestRate { get; set; } // Percentage
        public int TermYears { get; set; }
        public int TermMonths { get; set; }
        public DateTime StartDate { get; set; }

        // Extra Payments
        public decimal ExtraMonthlyPayment { get; set; }
        public decimal ExtraYearlyPayment { get; set; }
        public decimal ExtraOneTimePayment { get; set; }
        public DateTime? ExtraOneTimePaymentDate { get; set; }

        // Method to calculate monthly payment (OOP Behavior)
        public decimal CalculateMonthlyPayment()
        {
            double monthlyRate = (InterestRate / 100) / 12;
            int totalMonths = (TermYears * 12) + TermMonths;

            if (monthlyRate == 0) return LoanAmount / totalMonths;

            double payment = (double)LoanAmount * 
                (monthlyRate * Math.Pow(1 + monthlyRate, totalMonths)) / 
                (Math.Pow(1 + monthlyRate, totalMonths) - 1);

            return Math.Round((decimal)payment, 2);
        }

        public List<MonthlyAmortizationSchedule> GenerateMonthlySchedule()
        {
            var schedule = new List<MonthlyAmortizationSchedule>();
            decimal balance = LoanAmount;
            decimal monthlyPayment = CalculateMonthlyPayment();
            double monthlyRate = (InterestRate / 100) / 12;
            int totalMonths = (TermYears * 12) + TermMonths;
            DateTime currentDate = StartDate;

            for (int i = 1; i <= totalMonths; i++)
            {
                if (balance <= 0) break; // Loan paid off early

                decimal interest = Math.Round(balance * (decimal)monthlyRate, 2);
                decimal principal = monthlyPayment - interest;
                
                // Apply Regular Extra Monthly Payment
                decimal extra = ExtraMonthlyPayment;

                // Apply Extra Yearly Payment (e.g., every 12th month relative to start)
                if (i % 12 == 0)
                {
                    extra += ExtraYearlyPayment;
                }

                // Apply One-Time Payment
                if (ExtraOneTimePaymentDate.HasValue && 
                    currentDate.Month == ExtraOneTimePaymentDate.Value.Month && 
                    currentDate.Year == ExtraOneTimePaymentDate.Value.Year)
                {
                    extra += ExtraOneTimePayment;
                }

                principal += extra;

                // Adjustment for last payment
                if (balance < principal)
                {
                    principal = balance;
                    monthlyPayment = principal + interest; // Adjust payment to match remaining balance + interest
                }
                
                balance -= principal;

                schedule.Add(new MonthlyAmortizationSchedule
                {
                    Month = i,
                    Date = currentDate,
                    Payment = monthlyPayment + (balance == 0 ? 0 : extra), // Show total paid this month? 
                    // Actually, standard amortization tables usually show the scheduled payment + extra. 
                    // Let's keep it simple: Payment column = Total amount paid by user this month.
                    Principal = principal,
                    Interest = interest,
                    Balance = balance
                });

                currentDate = currentDate.AddMonths(1);
            }

            return schedule;
        }

        // Logic to aggregate into yearly schedule
        public List<AmortizationSchedule> GenerateYearlySchedule()
        {
            var monthlySchedule = GenerateMonthlySchedule();
            var yearlySchedule = new List<AmortizationSchedule>();
            
            // Group by year logic would basically sum up the monthly schedules
            // Implementation detail: simplified for now, can iterate monthlySchedule
            
            int currentYear = 0;
            decimal yearInterest = 0;
            decimal yearPrincipal = 0;
            decimal endingBalance = 0;
            DateTime yearStart = DateTime.MinValue;
            DateTime yearEnd = DateTime.MinValue;

            foreach (var month in monthlySchedule)
            {
                if (currentYear == 0) currentYear = month.Date.Year;

                if (month.Date.Year != currentYear)
                {
                    // new year, save previous
                    yearlySchedule.Add(new AmortizationSchedule
                    {
                        Year = currentYear, // This might need to be relative year (1, 2, 3...)
                        DateRange = $"{yearStart:MM/yy}-{yearEnd:MM/yy}",
                        Interest = yearInterest,
                        Principal = yearPrincipal,
                        EndingBalance = endingBalance,
                        TotalPayment = yearInterest + yearPrincipal
                    });

                    // Reset
                    currentYear = month.Date.Year;
                    yearInterest = 0;
                    yearPrincipal = 0;
                    yearStart = month.Date;
                }
                
                if (yearStart == DateTime.MinValue) yearStart = month.Date;
                yearEnd = month.Date;
                
                yearInterest += month.Interest;
                yearPrincipal += month.Principal;
                endingBalance = month.Balance;
            }

            // Add last year
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

            // Fix relative years
            int yearCount = 1;
            foreach(var item in yearlySchedule) {
                item.Year = yearCount++;
            }

            return yearlySchedule;
        }
    }
}
