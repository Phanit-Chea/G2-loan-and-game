using LoanSystem.WinForms.Domain;

namespace LoanSystem.WinForms.Data
{
    public class AppData
    {
        public List<AppUser> Users { get; set; } = new();
        public List<GameHistory> GameHistories { get; set; } = new();
        public List<LoanHistory> LoanHistories { get; set; } = new();
        public int NextGameHistoryId { get; set; } = 1;
        public int NextLoanHistoryId { get; set; } = 1;
    }
}
