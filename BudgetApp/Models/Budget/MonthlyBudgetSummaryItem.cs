// MonthlyBudgetSummaryItem.cs
namespace BudgetApp.Models.Budget
{
    public class MonthlyBudgetSummaryItem
    {
        public int BudgetPeriodId { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryType { get; set; } = string.Empty;  
        public decimal BudgetedAmount { get; set; }
        public decimal ActualAmount { get; set; }
    }
}

