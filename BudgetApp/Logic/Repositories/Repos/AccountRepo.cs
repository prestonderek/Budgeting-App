using BudgetApp.Data;
using BudgetApp.Models.Budget;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Logic.Repositories.Repos
{
    public class AccountRepo : IAccountRepository
    {
        private readonly ApplicationDbContext _db;

        public AccountRepo(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<Account>> GetAllAsync(string userId)
        {
            return await _db.Accounts
                .Where(a => a.UserId == userId && !a.IsClosed)
                .OrderBy(a => a.AccountName)
                .ToListAsync();
        }

        public async Task<Account?> GetByIdAsync(int accountId, string userId)
        {
            return await _db.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == accountId && a.UserId == userId);
        }

        public async Task CreateAsync(Account account)
        {
            _db.Accounts.Add(account);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Account account)
        {
            _db.Accounts.Update(account);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int accountId, string userId)
        {
            var entity = await GetByIdAsync(accountId, userId);
            if (entity == null)
                return;

            entity.IsClosed = true;
            _db.Accounts.Update(entity);
            await _db.SaveChangesAsync();
        }

        public async Task<Account?> GetMainCheckingAsync(string userId)
        {
            return await _db.Accounts
                .Where(a => a.UserId == userId
                    && a.AccountType == "Checking"
                    && !a.IsClosed)
                .OrderBy(a => a.AccountId)
                .FirstOrDefaultAsync();
        }

        public async Task<decimal> GetBalanceAsync(int accountId, string userId)
        {
            var account = await _db.Accounts
                .FirstOrDefaultAsync(a => a.AccountId == accountId && a.UserId == userId);

            if (account == null)
                return 0m;

            var transTotal = await _db.Transactions
                .Where(t => t.AccountId == accountId && t.UserId == userId)
                .SumAsync(t => (decimal?)t.Amount) ?? 0m;

            return account.StartingBalance + transTotal;
        }
    }
}

