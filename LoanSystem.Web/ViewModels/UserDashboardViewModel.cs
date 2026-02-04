using LoanSystem.Core.Models;
using System.Collections.Generic;

namespace LoanSystem.Web.ViewModels
{
    public class UserDashboardViewModel
    {
        public ApplicationUser User { get; set; }
        public List<GameHistory> GameHistory { get; set; }
        public List<LoanHistory> LoanHistory { get; set; }
    }
}
