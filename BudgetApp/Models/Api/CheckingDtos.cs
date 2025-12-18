namespace BudgetApp.Models.Api
{
    public class CheckingTransactionDtos
    {
        public int TransactionId { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal Amount { get; set; }
        public string? CategoryName { get; set; }
        public string? Memo { get; set; }
    }

    public class CheckingSummaryDto
    {
        public int AccountId { get; set; }
        public string AccountName { get; set; } = string.Empty;
        public decimal Balance { get; set; }

        public IReadOnlyList<CheckingTransactionDtos> RecentTransactions { get; set; }
            = Array.Empty<CheckingTransactionDtos>();
    }
}

