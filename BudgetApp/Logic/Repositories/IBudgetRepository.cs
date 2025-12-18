using BudgetApp.Models.Budget;

namespace BudgetApp.Logic.Repositories
{
    public interface IBudgetRepository
    {
        Task<BudgetPeriod?> GetOrCreatePeriodAsync(string userId, int year, int month);
        Task<IReadOnlyList<BudgetLine>> GetLinesAsync(int budgetPeriodId, string userId);
        Task SaveLinesAsync(int budgetPeriodId, string userId, IEnumerable<BudgetLine> lines);

        Task<IReadOnlyList<MonthlyBudgetSummaryItem>> GetMonthlySummaryAsync(string userId, int year, int month);
    }
}

