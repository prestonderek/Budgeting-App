namespace BudgetApp.Config
{
    public class BudgetSettings
    {
        public string Currency { get; set; } = "USD";
        public int MaxMonthsToDisplay { get; set; } = 12;
        public string SupportEmail { get; set; } = string.Empty;
    }
}
