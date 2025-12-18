using BudgetApp.Data;
using BudgetApp.Models.Budget;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Logic.Repositories.Repos
{
    public class CategoryRepo : ICategoryRepository
    {
        private readonly ApplicationDbContext _db;

        public CategoryRepo(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<Category>> GetAllAsync(string userId)
        {
            // Global (UserId == null) + user-specific
            return await _db.Categories
                .Where(c => c.UserId == null || c.UserId == userId)
                .Where(c => !c.IsArchived)
                .OrderBy(c => c.CategoryType)
                .ThenBy(c => c.DisplayOrder)
                .ThenBy(c => c.CategoryName)
                .ToListAsync();
        }

        public async Task<Category?> GetByIdAsync(int categoryId, string userId)
        {
            return await _db.Categories
                .FirstOrDefaultAsync(c =>
                    c.CategoryId == categoryId &&
                    (c.UserId == null || c.UserId == userId));
        }

        public async Task CreateAsync(Category category)
        {
            _db.Categories.Add(category);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Category category)
        {
            _db.Categories.Update(category);
            await _db.SaveChangesAsync();
        }

        public async Task ArchiveAsync(int categoryId, string userId)
        {
            var entity = await GetByIdAsync(categoryId, userId);
            if (entity == null)
                return;

            entity.IsArchived = true;
            _db.Categories.Update(entity);
            await _db.SaveChangesAsync();
        }
    }
}

