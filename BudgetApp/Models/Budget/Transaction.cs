using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetApp.Models.Budget
{
    [Table("Transactions")]
    public class Transaction
    {
        public int TransactionId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        public int AccountId { get; set; }

        public int? CategoryId { get; set; }

        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; }

        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }  

        [StringLength(255)]
        public string? Description { get; set; }

        public string? ExternalTransactionId { get; set; }
        public string? ExternalProvider { get; set; }
    }
}

