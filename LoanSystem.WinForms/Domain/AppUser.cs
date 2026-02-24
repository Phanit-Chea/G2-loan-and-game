namespace LoanSystem.WinForms.Domain
{
    public enum UserRole
    {
        User = 0,
        Admin = 1
    }

    public enum AccountStatus
    {
        Pending = 0,
        Active = 1,
        Rejected = 2,
        Terminated = 3,
        Deleted = 4
    }

    public class AppUser
    {
        public string Id { get; set; } = Guid.NewGuid().ToString("N");
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
        public AccountStatus Status { get; set; } = AccountStatus.Pending;
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public bool IsAdmin => Role == UserRole.Admin;

        public bool CanLogin()
        {
            return Status == AccountStatus.Active;
        }

        public void Approve()
        {
            if (!IsAdmin)
            {
                Status = AccountStatus.Active;
            }
        }

        public void Reject()
        {
            if (!IsAdmin)
            {
                Status = AccountStatus.Rejected;
            }
        }

        public void Terminate()
        {
            if (!IsAdmin)
            {
                Status = AccountStatus.Terminated;
            }
        }

        public void MarkDeleted()
        {
            if (!IsAdmin)
            {
                Status = AccountStatus.Deleted;
            }
        }
    }
}
