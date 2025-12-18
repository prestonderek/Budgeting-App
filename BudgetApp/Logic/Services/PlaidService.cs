using System.Net.Http.Json;
using BudgetApp.Config;
using BudgetApp.Data;
using BudgetApp.Models.Budget;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace BudgetApp.Logic.Services
{
    public class PlaidService : IPlaidService
    {
        //HELPER CLASSES
        private class PlaidAccountsResponse
        {
            public List<PlaidAccount> accounts { get; set; } = new();
        }

        private class PlaidAccount
        {
            public string account_id { get; set; } = string.Empty;
            public string name { get; set; } = string.Empty;
            public string? official_name { get; set; }
            public string type { get; set; } = string.Empty;    // "depository", "credit", etc.
            public string? subtype { get; set; }                // "checking", "savings", etc.
            public PlaidBalance balances { get; set; } = new();
        }

        private class PlaidBalance
        {
            public decimal? available { get; set; }
            public decimal? current { get; set; }
            public string? iso_currency_code { get; set; }
        }

        private class PlaidTransactionsResponse
        {
            public List<PlaidTransaction> transactions { get; set; } = new();
            public int total_transactions { get; set; }
        }

        private class PlaidTransaction
        {
            public string account_id { get; set; } = string.Empty;
            public string transaction_id { get; set; } = string.Empty;
            public decimal amount { get; set; }              // Plaid: positive for money out, negative for money in
            public string name { get; set; } = string.Empty; // description
            public string date { get; set; } = string.Empty; // "YYYY-MM-DD"
            public string? merchant_name { get; set; }
        }

        private readonly HttpClient _http;
        private readonly PlaidSettings _settings;
        private readonly ApplicationDbContext _db;

        public PlaidService(
            HttpClient http,
            IOptions<PlaidSettings> options,
            ApplicationDbContext db)
        {
            _http = http;
            _settings = options.Value;
            _db = db;

            _http.BaseAddress = new Uri(_settings.Environment switch
            {
                "sandbox" => "https://sandbox.plaid.com/",
                "development" => "https://development.plaid.com/",
                "production" => "https://production.plaid.com/",
                _ => "https://sandbox.plaid.com/"
            });
        }

        public async Task<string> CreateLinkTokenAsync(string userId)
        {
            var request = new
            {
                client_id = _settings.ClientId,
                secret = _settings.Secret,
                user = new { client_user_id = userId },
                client_name = "BudgetApp",
                products = _settings.Products,
                country_codes = _settings.CountryCodes,
                language = "en",
                redirect_uri = (string?)null
            };

            var response = await _http.PostAsJsonAsync("link/token/create", request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<Dictionary<string, object?>>();
            if (json is null || !json.TryGetValue("link_token", out var linkTokenObj) || linkTokenObj is null)
                throw new InvalidOperationException("Failed to create Plaid link token.");

            return linkTokenObj.ToString()!;
        }

        public async Task<(string AccessToken, string ItemId)> ExchangePublicTokenAsync(string publicToken)
        {
            var request = new
            {
                client_id = _settings.ClientId,
                secret = _settings.Secret,
                public_token = publicToken
            };

            var response = await _http.PostAsJsonAsync("item/public_token/exchange", request);
            response.EnsureSuccessStatusCode();

            var json = await response.Content.ReadFromJsonAsync<Dictionary<string, object?>>();
            if (json is null)
                throw new InvalidOperationException("Failed to exchange public token.");

            var accessToken = json["access_token"]?.ToString() ?? "";
            var itemId = json["item_id"]?.ToString() ?? "";

            if (string.IsNullOrEmpty(accessToken) || string.IsNullOrEmpty(itemId))
                throw new InvalidOperationException("Plaid did not return access_token or item_id.");

            return (accessToken, itemId);
        }

        public async Task SyncAccountsAndTransactionsAsync(string userId, int bankLinkId)
        {
            var link = await _db.BankLinks
                .FirstOrDefaultAsync(b => b.BankLinkId == bankLinkId && b.UserId == userId && b.IsActive);

            if (link == null)
                throw new InvalidOperationException("Bank link not found or not active.");

            var accessToken = link.AccessToken;

            // 1) Sync accounts
            var accountsRequest = new
            {
                client_id = _settings.ClientId,
                secret = _settings.Secret,
                access_token = accessToken
            };

            var accountsResponse = await _http.PostAsJsonAsync("accounts/get", accountsRequest);
            var accountsJson = await accountsResponse.Content.ReadAsStringAsync();
            accountsResponse.EnsureSuccessStatusCode();

            var acctPayload = System.Text.Json.JsonSerializer.Deserialize<PlaidAccountsResponse>(accountsJson);
            if (acctPayload == null)
                throw new InvalidOperationException("Failed to parse Plaid accounts response.");

            foreach (var pa in acctPayload.accounts)
            {
                var externalId = pa.account_id;
                var name = !string.IsNullOrWhiteSpace(pa.official_name)
                    ? pa.official_name
                    : pa.name;

                var account = await _db.Accounts
                    .FirstOrDefaultAsync(a =>
                        a.UserId == userId &&
                        a.ExternalProvider == "Plaid" &&
                        a.ExternalAccountId == externalId);

                if (account == null)
                {
                    account = new Account
                    {
                        UserId = userId,
                        AccountName = name,
                        AccountType = MapAccountType(pa),
                        StartingBalance = pa.balances.current ?? 0m,
                        IsClosed = false,
                        ExternalAccountId = externalId,
                        ExternalProvider = "Plaid"
                    };

                    _db.Accounts.Add(account);
                }
                else
                {
                    // update name/type/balance if they changed
                    account.AccountName = name;
                    account.AccountType = MapAccountType(pa);
                    account.ExternalProvider = "Plaid";
                    account.ExternalAccountId = externalId;
                }
            }

            await _db.SaveChangesAsync();

            //Lookup and wire external to internal account IDs
            var localAccounts = await _db.Accounts
                .Where(a => a.UserId == userId && a.ExternalProvider == "Plaid")
                .ToListAsync();

            var accountMap = localAccounts
                .Where(a => a.ExternalAccountId != null)
                .ToDictionary(a => a.ExternalAccountId!, a => a.AccountId);

            //Last 30 days of transactions
            var endDate = DateTime.Today;
            var startDate = endDate.AddDays(-30);

            var txRequest = new
            {
                client_id = _settings.ClientId,
                secret = _settings.Secret,
                access_token = accessToken,
                start_date = startDate.ToString("yyyy-MM-dd"),
                end_date = endDate.ToString("yyyy-MM-dd"),
                options = new
                {
                    count = 100,
                    offset = 0
                }
            };

            var txResponse = await _http.PostAsJsonAsync("transactions/get", txRequest);
            var txJson = await txResponse.Content.ReadAsStringAsync();
            txResponse.EnsureSuccessStatusCode();

            var txPayload = System.Text.Json.JsonSerializer.Deserialize<PlaidTransactionsResponse>(txJson);
            if (txPayload == null)
                throw new InvalidOperationException("Failed to parse Plaid transactions response.");

            foreach (var pt in txPayload.transactions)
            {
                if (!accountMap.TryGetValue(pt.account_id, out var localAccountId))
                {
                    continue;
                }

                var existing = await _db.Transactions
                    .FirstOrDefaultAsync(t =>
                        t.UserId == userId &&
                        t.ExternalProvider == "Plaid" &&
                        t.ExternalTransactionId == pt.transaction_id);

                if (existing != null)
                {
                    continue;
                }

                var date = DateTime.Parse(pt.date);

                var tx = new Transaction
                {
                    UserId = userId,
                    AccountId = localAccountId,
                    Amount = pt.amount, 
                    Description = string.IsNullOrWhiteSpace(pt.merchant_name)
                        ? pt.name
                        : pt.merchant_name,
                    CreatedAt = date,
                    ExternalProvider = "Plaid",
                    ExternalTransactionId = pt.transaction_id
                };

                _db.Transactions.Add(tx);
            }

            await _db.SaveChangesAsync();
        }


        //HELPER METHODS

        private static string MapAccountType(PlaidAccount a)
        {
            var subtype = a.subtype?.ToLowerInvariant();
            return subtype switch
            {
                "checking" => "Checking",
                "savings" => "Savings",
                "credit card" => "CreditCard",
                "credit" => "CreditCard",
                _ => a.type
            };
        }


    }
}

