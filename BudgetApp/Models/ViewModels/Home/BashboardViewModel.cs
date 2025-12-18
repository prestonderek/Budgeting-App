using BudgetApp.Models.Api;

namespace BudgetApp.Models.ViewModels.Home
{
    public class DashboardViewModel
    {
        public decimal CheckingBalance { get; set; }
        public bool HasChecking { get; set; }

        public decimal MonthIncome { get; set; }
        public decimal MonthExpenses { get; set; }

        public decimal MonthNet => MonthIncome + MonthExpenses;

        public IReadOnlyList<CheckingTransactionDtos> RecentTransactions { get; set; }
            = Array.Empty<CheckingTransactionDtos>();
    }
}

