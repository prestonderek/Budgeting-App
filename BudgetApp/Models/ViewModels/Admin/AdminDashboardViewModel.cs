using System.Collections.Generic;

namespace BudgetApp.Models.ViewModels.Admin
{
    public class AdminUserItem
    {
        public string UserId { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public IList<string> Roles { get; set; } = new List<string>();
    }

    public class AdminDashboardViewModel
    {
        public int TotalUsers { get; set; }
        public int TotalAccounts { get; set; }
        public int TotalTransactions { get; set; }

        public List<AdminUserItem> Users { get; set; } = new();
    }
}

