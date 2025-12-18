namespace BudgetApp.Models.ViewModels.Accounts
{
    public class AccountListItemViewModel
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public string AccountType { get; set; } = string.Empty;
        public decimal Balance { get; set; }
        public bool IsClosed { get; set; }
    }
}

