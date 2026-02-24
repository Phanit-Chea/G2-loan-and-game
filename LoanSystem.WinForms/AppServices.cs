using LoanSystem.WinForms.Data;
using LoanSystem.WinForms.Services;

namespace LoanSystem.WinForms
{
    public class AppServices
    {
        public IAppDataStore DataStore { get; }
        public AuthService AuthService { get; }
        public AdminService AdminService { get; }
        public LoanService LoanService { get; }
        public GameService GameService { get; }
        public UserDashboardService UserDashboardService { get; }
        public AppSession Session { get; }

        public AppServices()
        {
            DataStore = new AppDataStore();
            AuthService = new AuthService(DataStore);
            AdminService = new AdminService(DataStore);
            LoanService = new LoanService(DataStore);
            GameService = new GameService(DataStore);
            UserDashboardService = new UserDashboardService(DataStore);
            Session = new AppSession();
        }
    }
}
