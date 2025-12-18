using BudgetApp.Models.Budget;
using Microsoft.Extensions.Caching.Memory;

namespace BudgetApp.Logic.Repositories.CachedRepos
{
    public class CachedAccountRepo : IAccountRepository
    {
        private readonly IAccountRepository _repo;
        private readonly IMemoryCache _cache;

        public CachedAccountRepo(IAccountRepository repo, IMemoryCache cache)
        {
            _repo = repo;
            _cache = cache;
        }

        private static string CacheKey(string userId) => $"accounts:{userId}";

        public async Task<IReadOnlyList<Account>> GetAllAsync(string userId)
        {
            var key = CacheKey(userId);

            if (_cache.TryGetValue<IReadOnlyList<Account>>(key, out var cached))
                return cached;

            var accounts = await _repo.GetAllAsync(userId);

            _cache.Set(key, accounts, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            return accounts;
        }

        public async Task<Account?> GetByIdAsync(int accountId, string userId)
        {
            var all = await GetAllAsync(userId);
            return all.FirstOrDefault(a => a.AccountId == accountId);
        }

        public async Task CreateAsync(Account account)
        {
            await _repo.CreateAsync(account);
            _cache.Remove(CacheKey(account.UserId));
        }

        public async Task UpdateAsync(Account account)
        {
            await _repo.UpdateAsync(account);
            _cache.Remove(CacheKey(account.UserId));
        }

        public async Task DeleteAsync(int accountId, string userId)
        {
            await _repo.DeleteAsync(accountId, userId);
            _cache.Remove(CacheKey(userId));
        }

        public async Task<Account?> GetMainCheckingAsync(string userId)
        {
            var all = await GetAllAsync(userId);
            return all
                .Where(a => a.AccountType == "Checking")
                .OrderBy(a => a.AccountId)
                .FirstOrDefault();

        }

        public async Task<decimal> GetBalanceAsync(int accountId, string userId)
        {
            return await _repo.GetBalanceAsync(accountId, userId);
        }
    }
}

