using LoanSystem.WinForms.Data;
using LoanSystem.WinForms.Domain;

namespace LoanSystem.WinForms.Services
{
    public class UserDashboardService
    {
        private readonly IAppDataStore _store;

        public UserDashboardService(IAppDataStore store)
        {
            _store = store;
        }

        public List<GameHistory> GetGameHistory(string userId, int take = 20)
        {
            return _store.GetGameHistoryForUser(userId, take);
        }

        public List<LoanHistory> GetLoanHistory(string userId, int take = 20)
        {
            return _store.GetLoanHistoryForUser(userId, take);
        }
    }
}
