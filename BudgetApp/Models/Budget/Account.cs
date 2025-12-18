using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetApp.Models.Budget
{
    [Table("Accounts")]
    public class Account
    {
        public int AccountId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required, StringLength(100)]
        public string AccountName { get; set; } = string.Empty;

        [Required, StringLength(50)]
        public string AccountType { get; set; } = string.Empty;

        [DataType(DataType.Currency)]
        public decimal StartingBalance { get; set; }

        public bool IsClosed { get; set; }

        public DateTime CreatedAt { get; set; }

        public string? ExternalAccountId { get; set; }

        public string? ExternalProvider { get; set; }
    }
}
