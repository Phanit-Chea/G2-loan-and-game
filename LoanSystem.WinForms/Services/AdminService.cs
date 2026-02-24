using LoanSystem.WinForms.Data;
using LoanSystem.WinForms.Domain;

namespace LoanSystem.WinForms.Services
{
    public class AdminService
    {
        private readonly IAppDataStore _store;

        public AdminService(IAppDataStore store)
        {
            _store = store;
        }

        public List<AppUser> GetUsers()
        {
            return _store.GetUsers();
        }

        public void ApproveUser(string userId)
        {
            var user = _store.GetUserById(userId);
            if (user == null)
            {
                return;
            }

            user.Approve();
            _store.UpdateUser(user);
        }

        public void RejectUser(string userId)
        {
            var user = _store.GetUserById(userId);
            if (user == null)
            {
                return;
            }

            user.Reject();
            _store.UpdateUser(user);
        }

        public void TerminateUser(string userId)
        {
            var user = _store.GetUserById(userId);
            if (user == null)
            {
                return;
            }

            user.Terminate();
            _store.UpdateUser(user);
        }

        public void DeleteUser(string userId)
        {
            var user = _store.GetUserById(userId);
            if (user == null)
            {
                return;
            }

            user.MarkDeleted();
            _store.UpdateUser(user);
        }
    }
}
