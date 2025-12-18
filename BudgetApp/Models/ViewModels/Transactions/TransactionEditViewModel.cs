using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;

namespace BudgetApp.Models.ViewModels.Transactions
{
    public class TransactionEditViewModel
    {
        public int? TransactionId { get; set; }

        [Required]
        public int AccountId { get; set; }

        public int? CategoryId { get; set; }

        [Required]
        [DataType(DataType.Date)]
        public DateTime CreatedAt { get; set; } = DateTime.Today;

        [Required]
        [DataType(DataType.Currency)]
        public decimal Amount { get; set; }

        [StringLength(255)]
        public string? Description { get; set; }

        // for dropdowns
        public IEnumerable<SelectListItem>? AccountOptions { get; set; }
        public IEnumerable<SelectListItem>? CategoryOptions { get; set; }
    }
}

