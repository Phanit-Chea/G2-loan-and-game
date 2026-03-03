namespace LoanSystem.WinForms.Domain.Games
{
    public readonly record struct GameOutcome(string GameName, int Score, string Result);

    public interface IGameSession
    {
        string GameName { get; }
        bool IsRunning { get; }
        int Score { get; }
        GameOutcome BuildOutcome();
    }

    public abstract class GameSessionBase : IGameSession
    {
        public abstract string GameName { get; }
        public bool IsRunning { get; protected set; }
        public int Score { get; protected set; }
        public abstract GameOutcome BuildOutcome();
    }
}
