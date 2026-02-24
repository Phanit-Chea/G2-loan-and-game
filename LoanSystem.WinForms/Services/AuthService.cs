using LoanSystem.WinForms.Data;
using LoanSystem.WinForms.Domain;
using LoanSystem.WinForms.Security;

namespace LoanSystem.WinForms.Services
{
    public class AuthService
    {
        private readonly IAppDataStore _store;

        public AuthService(IAppDataStore store)
        {
            _store = store;
        }

        public (bool Success, string Message) Register(string fullName, string email, string password, string confirmPassword)
        {
            if (string.IsNullOrWhiteSpace(fullName) || string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                return (false, "All fields are required.");
            }

            if (password.Length < 6)
            {
                return (false, "Password must be at least 6 characters.");
            }

            if (!password.Equals(confirmPassword))
            {
                return (false, "Password and confirmation password do not match.");
            }

            if (_store.GetUserByEmail(email) != null)
            {
                return (false, "Email is already registered.");
            }

            var user = new AppUser
            {
                FullName = fullName.Trim(),
                Email = email.Trim(),
                PasswordHash = PasswordHashing.Hash(password),
                Role = UserRole.User,
                Status = AccountStatus.Pending
            };

            _store.AddUser(user);
            return (true, "Registration successful. Please wait for admin approval.");
        }

        public (AppUser? User, string Message) Login(string email, string password)
        {
            var user = _store.GetUserByEmail(email.Trim());
            if (user == null || !PasswordHashing.Verify(password, user.PasswordHash))
            {
                return (null, "Invalid login attempt.");
            }

            if (!user.CanLogin())
            {
                return (null, "Your account is not active. Please wait for admin approval.");
            }

            return (user, string.Empty);
        }
    }
}
