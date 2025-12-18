using BudgetApp.Data;
using BudgetApp.Models.Budget;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Logic.Repositories.Repos
{
    public class TransactionRepo : ITransactionRepository
    {
        private readonly ApplicationDbContext _db;

        public TransactionRepo(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<IReadOnlyList<Transaction>> GetForPeriodAsync(string userId, DateTime from, DateTime to)
        {
            return await _db.Transactions
                .Where(t => t.UserId == userId &&
                            t.CreatedAt >= from.Date &&
                            t.CreatedAt <= to.Date)
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.TransactionId)
                .ToListAsync();
        }

        public async Task<Transaction?> GetByIdAsync(int transactionId, string userId)
        {
            return await _db.Transactions
                .FirstOrDefaultAsync(t => t.TransactionId == transactionId && t.UserId == userId);
        }

        public async Task CreateAsync(Transaction transaction)
        {
            _db.Transactions.Add(transaction);
            await _db.SaveChangesAsync();
        }

        public async Task UpdateAsync(Transaction transaction)
        {
            _db.Transactions.Update(transaction);
            await _db.SaveChangesAsync();
        }

        public async Task DeleteAsync(int transactionId, string userId)
        {
            var entity = await GetByIdAsync(transactionId, userId);
            if (entity == null)
                return;

            _db.Transactions.Remove(entity);
            await _db.SaveChangesAsync();
        }
    }
}

