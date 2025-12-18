using BudgetApp.Models.Budget;

namespace BudgetApp.Logic.Repositories
{
    public interface IAccountRepository
    {
        Task<IReadOnlyList<Account>> GetAllAsync(string userId);
        Task<Account?> GetByIdAsync(int accountId, string userId);
        Task CreateAsync(Account account);
        Task UpdateAsync(Account account);
        Task DeleteAsync(int accountId, string userId);

        //Pulling main checking methods
        Task<Account?> GetMainCheckingAsync(string userId);
        Task<decimal> GetBalanceAsync(int accountId, string userId);
    }
}
