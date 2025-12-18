using System.Data;
using BudgetApp.Data;
using BudgetApp.Models.Budget;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Logic.Repositories.Repos
{
    public class BudgetRepo : IBudgetRepository
    {
        private readonly ApplicationDbContext _db;

        public BudgetRepo(ApplicationDbContext db)
        {
            _db = db;
        }

        public async Task<BudgetPeriod?> GetOrCreatePeriodAsync(string userId, int year, int month)
        {
            var existing = await _db.BudgetPeriods
                .FirstOrDefaultAsync(bp =>
                    bp.UserId == userId &&
                    bp.Year == year &&
                    bp.Month == month);

            if (existing != null)
                return existing;

            var period = new BudgetPeriod
            {
                UserId = userId,
                Year = year,
                Month = month,
                CreatedAt = DateTime.UtcNow
            };

            _db.BudgetPeriods.Add(period);
            await _db.SaveChangesAsync();

            return period;
        }

        public async Task<IReadOnlyList<BudgetLine>> GetLinesAsync(int budgetPeriodId, string userId)
        {
            var period = await _db.BudgetPeriods
                .FirstOrDefaultAsync(bp => bp.BudgetPeriodId == budgetPeriodId && bp.UserId == userId);

            if (period == null)
                return Array.Empty<BudgetLine>();

            return await _db.BudgetLines
                .Where(bl => bl.BudgetPeriodId == budgetPeriodId)
                .OrderBy(bl => bl.BudgetLineId)
                .ToListAsync();
        }

        public async Task SaveLinesAsync(int budgetPeriodId, string userId, IEnumerable<BudgetLine> lines)
        {
            var period = await _db.BudgetPeriods
                .FirstOrDefaultAsync(bp => bp.BudgetPeriodId == budgetPeriodId && bp.UserId == userId);

            if (period == null)
                throw new InvalidOperationException("Budget period does not exist or does not belong to user.");

            var existing = _db.BudgetLines.Where(bl => bl.BudgetPeriodId == budgetPeriodId);
            _db.BudgetLines.RemoveRange(existing);

            foreach (var line in lines)
            {
                line.BudgetPeriodId = budgetPeriodId;
                _db.BudgetLines.Add(line);
            }

            await _db.SaveChangesAsync();
        }

        public async Task<IReadOnlyList<MonthlyBudgetSummaryItem>> GetMonthlySummaryAsync(string userId, int year, int month)
        {
            var result = new List<MonthlyBudgetSummaryItem>();

            var conn = _db.Database.GetDbConnection();
            await using (conn)
            {
                if (conn.State != ConnectionState.Open)
                    await conn.OpenAsync();

                await using var cmd = conn.CreateCommand();
                cmd.CommandText = "dbo.GetMonthlyBudgetSummary";
                cmd.CommandType = CommandType.StoredProcedure;

                var pUser = cmd.CreateParameter();
                pUser.ParameterName = "@UserId";
                pUser.Value = userId;
                cmd.Parameters.Add(pUser);

                var pYear = cmd.CreateParameter();
                pYear.ParameterName = "@Year";
                pYear.Value = year;
                cmd.Parameters.Add(pYear);

                var pMonth = cmd.CreateParameter();
                pMonth.ParameterName = "@Month";
                pMonth.Value = month;
                cmd.Parameters.Add(pMonth);

                await using var reader = await cmd.ExecuteReaderAsync();
                while (await reader.ReadAsync())
                {
                    var item = new MonthlyBudgetSummaryItem
                    {
                        BudgetPeriodId = reader.GetInt32(reader.GetOrdinal("BudgetPeriodId")),
                        CategoryId = reader.GetInt32(reader.GetOrdinal("CategoryId")),
                        CategoryName = reader.GetString(reader.GetOrdinal("CategoryName")),
                        CategoryType = reader.GetString(reader.GetOrdinal("CategoryType")),
                        BudgetedAmount = reader.GetDecimal(reader.GetOrdinal("BudgetedAmount")),
                        ActualAmount = reader.GetDecimal(reader.GetOrdinal("ActualAmount"))
                    };

                    result.Add(item);
                }
            }

            return result;
        }
    }
}

