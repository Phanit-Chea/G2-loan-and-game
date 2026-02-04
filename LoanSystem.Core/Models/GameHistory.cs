using System;

namespace LoanSystem.Core.Models
{
    public class GameHistory
    {
        public int Id { get; set; }
        public string UserId { get; set; }
        public virtual ApplicationUser User { get; set; }

        public string GameName { get; set; } // TicTacToe, CarRacing
        public int Score { get; set; }
        public DateTime PlayedAt { get; set; } = DateTime.Now;
        public string Result { get; set; } // Win, Loss, Draw, or detailed score info
    }
}
