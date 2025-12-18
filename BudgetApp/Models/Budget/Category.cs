using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BudgetApp.Models.Budget
{
    [Table("Categories")]
    public class Category
    {
        public int CategoryId { get; set; }

        public string? UserId { get; set; }  

        [Required, StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required, StringLength(20)]
        public string CategoryType { get; set; } = string.Empty; 

        public bool IsArchived { get; set; }

        public int DisplayOrder { get; set; }
    }
}
