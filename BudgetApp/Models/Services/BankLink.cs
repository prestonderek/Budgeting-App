namespace BudgetApp.Models.Services
{
    public class BankLink
    {
        public int BankLinkId { get; set; }
        public string UserId { get; set; } = string.Empty;
        public string Provider { get; set; } = "Plaid";
        public string ItemId { get; set; } = string.Empty;
        public string AccessToken { get; set; } = string.Empty;
        public string? InstitutionName { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
    }

}
