namespace LoanSystem.WinForms.Domain
{
    public class LoanHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public decimal Amount { get; set; }
        public double InterestRate { get; set; }
        public int TermYears { get; set; }
        public int TermMonths { get; set; }
        public decimal TotalPayment { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;
    }
}
