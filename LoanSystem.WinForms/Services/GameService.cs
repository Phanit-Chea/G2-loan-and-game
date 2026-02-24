using LoanSystem.WinForms.Data;
using LoanSystem.WinForms.Domain;

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
    }
}
