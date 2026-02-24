using LoanSystem.WinForms.Domain;

namespace LoanSystem.WinForms.Data
{
    public interface IAppDataStore
    {
        List<AppUser> GetUsers();
        AppUser? GetUserByEmail(string email);
        AppUser? GetUserById(string userId);
        void AddUser(AppUser user);
        void UpdateUser(AppUser user);
        List<GameHistory> GetGameHistoryForUser(string userId, int take = 20);
        List<LoanHistory> GetLoanHistoryForUser(string userId, int take = 20);
        void AddGameHistory(GameHistory history);
        void AddLoanHistory(LoanHistory history);
    }
}
