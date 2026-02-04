using Microsoft.AspNetCore.Identity;
using System.Collections.Generic;

namespace LoanSystem.Core.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public string AccountStatus { get; set; } = "Pending"; // Pending, Active, Rejected

        // Navigation property for game history
        public virtual ICollection<GameHistory> GameHistories { get; set; }
    }
}
