using System.ComponentModel.DataAnnotations;

namespace BudgetApp.Models.ViewModels.Accounts
{
    public class AccountEditViewModel
    {
        public int? AccountId { get; set; }

        [Required, StringLength(100)]
        public string AccountName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string AccountType { get; set; } = "Checking";

        [DataType(DataType.Currency)]
        public decimal StartingBalance { get; set; }
    }
}

