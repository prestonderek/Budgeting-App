using Azure.Core;

namespace BudgetApp.Logic.Services
{
    public interface IPlaidService
    {
        Task<string> CreateLinkTokenAsync(string userId);

        Task<(string AccessToken, string ItemId)> ExchangePublicTokenAsync(string publicToken);

        Task SyncAccountsAndTransactionsAsync(string userId, int bankLinkId);
    }
}
