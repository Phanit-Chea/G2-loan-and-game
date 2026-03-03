namespace LoanSystem.WinForms.Domain.Games
{
    public enum TicTacToeMode
    {
        VsBot = 0,
        VsFriend = 1
    }

    public enum TicTacToeOutcome
    {
        InProgress = 0,
        XWins = 1,
        OWins = 2,
        Draw = 3
    }

    public class TicTacToeGame : GameSessionBase
    {
        private static readonly int[][] WinConditions =
        {
            new[] { 0, 1, 2 }, new[] { 3, 4, 5 }, new[] { 6, 7, 8 },
            new[] { 0, 3, 6 }, new[] { 1, 4, 7 }, new[] { 2, 5, 8 },
            new[] { 0, 4, 8 }, new[] { 2, 4, 6 }
        };

        private readonly string[] _board = new string[9];

        public override string GameName => "TicTacToe";
        public IReadOnlyList<string> Board => _board;
        public string CurrentPlayer { get; private set; } = "X";
        public TicTacToeMode Mode { get; private set; } = TicTacToeMode.VsBot;
        public TicTacToeOutcome Outcome { get; private set; } = TicTacToeOutcome.InProgress;
        public bool IsComputerTurn => IsRunning && Mode == TicTacToeMode.VsBot && CurrentPlayer == "O";

        public void SetMode(TicTacToeMode mode)
        {
            Mode = mode;
        }

        public void Reset()
        {
            Array.Fill(_board, string.Empty);
            CurrentPlayer = "X";
            Outcome = TicTacToeOutcome.InProgress;
            Score = 0;
            IsRunning = true;
        }

        public bool TryPlay(int index)
        {
            if (index < 0 || index >= _board.Length || !IsRunning || !string.IsNullOrEmpty(_board[index]))
            {
                return false;
            }

            _board[index] = CurrentPlayer;
            EvaluateOutcome();

            if (IsRunning)
            {
                CurrentPlayer = CurrentPlayer == "X" ? "O" : "X";
            }

            return true;
        }

        public int? PlayComputerMove()
        {
            if (!IsComputerTurn)
            {
                return null;
            }

            var emptyIndices = Enumerable.Range(0, _board.Length)
                .Where(i => string.IsNullOrEmpty(_board[i]))
                .ToList();

            if (!emptyIndices.Any())
            {
                return null;
            }

            var selectedIndex = emptyIndices[Random.Shared.Next(emptyIndices.Count)];
            return TryPlay(selectedIndex) ? selectedIndex : null;
        }

        public string GetStatusText()
        {
            return Outcome switch
            {
                TicTacToeOutcome.XWins => "Player X Wins!",
                TicTacToeOutcome.OWins => "Player O Wins!",
                TicTacToeOutcome.Draw => "Draw!",
                _ => $"Player {CurrentPlayer}'s Turn"
            };
        }

        public (int Score, string Result) GetHumanPlayerResult()
        {
            if (Mode == TicTacToeMode.VsFriend)
            {
                return Outcome switch
                {
                    TicTacToeOutcome.XWins => (10, "Friend Match: X Wins"),
                    TicTacToeOutcome.OWins => (10, "Friend Match: O Wins"),
                    TicTacToeOutcome.Draw => (5, "Friend Match: Draw"),
                    _ => (0, "InProgress")
                };
            }

            return Outcome switch
            {
                TicTacToeOutcome.XWins => (10, "Win"),
                TicTacToeOutcome.OWins => (0, "Loss"),
                TicTacToeOutcome.Draw => (5, "Draw"),
                _ => (0, "InProgress")
            };
        }

        public override GameOutcome BuildOutcome()
        {
            var (score, result) = GetHumanPlayerResult();
            Score = score;
            return new GameOutcome(GameName, score, result);
        }

        private void EvaluateOutcome()
        {
            var hasWinner = WinConditions.Any(condition =>
                !string.IsNullOrEmpty(_board[condition[0]]) &&
                _board[condition[0]] == _board[condition[1]] &&
                _board[condition[1]] == _board[condition[2]]);

            if (hasWinner)
            {
                Outcome = CurrentPlayer == "X" ? TicTacToeOutcome.XWins : TicTacToeOutcome.OWins;
                FinalizeGame();
                return;
            }

            if (_board.All(x => !string.IsNullOrEmpty(x)))
            {
                Outcome = TicTacToeOutcome.Draw;
                FinalizeGame();
            }
        }

        private void FinalizeGame()
        {
            IsRunning = false;
            Score = GetHumanPlayerResult().Score;
        }
    }
}
