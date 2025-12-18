using BudgetApp.Models.Budget;

namespace BudgetApp.Logic.Repositories
{
    public interface ITransactionRepository
    {
        Task<IReadOnlyList<Transaction>> GetForPeriodAsync(string userId, DateTime from, DateTime to);
        Task<Transaction?> GetByIdAsync(int transactionId, string userId);
        Task CreateAsync(Transaction transaction);
        Task UpdateAsync(Transaction transaction);
        Task DeleteAsync(int transactionId, string userId);
    }
}

