namespace BudgetApp.Config
{
    public class PlaidSettings
    {
        public string Environment { get; set; } = "sandbox";
        public string ClientId { get; set; } = string.Empty;
        public string Secret { get; set; } = string.Empty;
        public string[] Products { get; set; } = Array.Empty<string>();
        public string[] CountryCodes { get; set; } = new[] { "US" };
    }
}

