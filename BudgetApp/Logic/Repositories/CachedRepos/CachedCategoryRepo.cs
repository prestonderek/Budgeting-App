using BudgetApp.Models.Budget;
using Microsoft.Extensions.Caching.Memory;

namespace BudgetApp.Logic.Repositories.CachedRepos
{
    public class CachedCategoryRepo : ICategoryRepository
    {
        private readonly ICategoryRepository _repo;
        private readonly IMemoryCache _cache;

        public CachedCategoryRepo(ICategoryRepository repo, IMemoryCache cache)
        {
            _repo = repo;
            _cache = cache;
        }

        private static string CacheKey(string userId) => $"categories:{userId}";

        public async Task<IReadOnlyList<Category>> GetAllAsync(string userId)
        {
            var key = CacheKey(userId);

            if (_cache.TryGetValue<IReadOnlyList<Category>>(key, out var cached))
                return cached;

            var categories = await _repo.GetAllAsync(userId);

            _cache.Set(key, categories, new MemoryCacheEntryOptions
            {
                AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5)
            });

            return categories;
        }

        public async Task<Category?> GetByIdAsync(int categoryId, string userId)
        {
            var all = await GetAllAsync(userId);
            return all.FirstOrDefault(c => c.CategoryId == categoryId);
        }

        public async Task CreateAsync(Category category)
        {
            await _repo.CreateAsync(category);
            _cache.Remove(CacheKey(category.UserId ?? string.Empty));
        }

        public async Task UpdateAsync(Category category)
        {
            await _repo.UpdateAsync(category);
            _cache.Remove(CacheKey(category.UserId ?? string.Empty));
        }

        public async Task ArchiveAsync(int categoryId, string userId)
        {
            await _repo.ArchiveAsync(categoryId, userId);
            _cache.Remove(CacheKey(userId));
        }
    }
}

