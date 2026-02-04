using LoanSystem.Core.Models;
using System.Collections.Generic;

namespace LoanSystem.Web.ViewModels
{
    public class AdminDashboardViewModel
    {
        public List<ApplicationUser> Users { get; set; }
        public List<string> Games { get; set; }
    }
}
