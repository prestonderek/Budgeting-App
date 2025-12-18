using BudgetApp.Models.Budget;

namespace BudgetApp.Logic.Repositories
{
    public interface ICategoryRepository
    {
        Task<IReadOnlyList<Category>> GetAllAsync(string userId);
        Task<Category?> GetByIdAsync(int categoryId, string userId);
        Task CreateAsync(Category category);
        Task UpdateAsync(Category category);
        Task ArchiveAsync(int categoryId, string userId);
    }
}

