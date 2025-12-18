using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetApp.Models.Budget
{
    [Table("BudgetLines")]
    public class BudgetLine
    {
        public int BudgetLineId { get; set; }

        [Required]
        public int BudgetPeriodId { get; set; }

        [Required]
        public int CategoryId { get; set; }

        [DataType(DataType.Currency)]
        public decimal BudgetedAmount { get; set; }

        [StringLength(255)]
        public string? Notes { get; set; }
    }
}

