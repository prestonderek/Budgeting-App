namespace BudgetApp.Models.ViewModels.Budget
{
    public class BudgetLineRow
    {
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string CategoryType { get; set; } = string.Empty;
        public decimal BudgetedAmount { get; set; } 
        public decimal ActualAmount { get; set; }
    }

    public class BudgetEditViewModel
    {
        public int Year { get; set; }
        public int Month { get; set; }
        public int BudgetPeriodId { get; set; }

        public List<BudgetLineRow> Lines { get; set; } = new();
    }
}

