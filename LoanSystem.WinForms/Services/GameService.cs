using LoanSystem.WinForms.Data;
using LoanSystem.WinForms.Domain;
using LoanSystem.WinForms.Domain.Games;

namespace LoanSystem.WinForms.Services
{
    public class GameService
    {
        private readonly IAppDataStore _store;

        public GameService(IAppDataStore store)
        {
            _store = store;
        }

        public void SaveScore(string userId, string gameName, int score, string result)
        {
            var history = new GameHistory
            {
                UserId = userId,
                GameName = gameName,
                Score = score,
                Result = result,
                PlayedAt = DateTime.Now
            };

            _store.AddGameHistory(history);
        }

        public void SaveScore(string userId, IGameSession game)
        {
            var outcome = game.BuildOutcome();
            SaveScore(userId, outcome.GameName, outcome.Score, outcome.Result);
        }
    }
}
