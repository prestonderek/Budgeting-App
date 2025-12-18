using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetApp.Models.Budget
{
    [Table("BudgetPeriods")]
    public class BudgetPeriod
    {
        public int BudgetPeriodId { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Range(1900, 3000)]
        public int Year { get; set; }

        [Range(1, 12)]
        public int Month { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
