namespace LoanSystem.WinForms.Domain
{
    public class GameHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string GameName { get; set; } = string.Empty;
        public int Score { get; set; }
        public string Result { get; set; } = string.Empty;
        public DateTime PlayedAt { get; set; } = DateTime.Now;
    }
}
