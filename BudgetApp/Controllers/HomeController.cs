using System.Security.Claims;
using BudgetApp.Logic.Repositories;
using BudgetApp.Models.Api;
using BudgetApp.Models.ViewModels.Home;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

public class HomeController : Controller
{
    private readonly IAccountRepository _accounts;
    private readonly ITransactionRepository _transactions;

    public HomeController(IAccountRepository accounts, ITransactionRepository transactions)
    {
        _accounts = accounts;
        _transactions = transactions;
    }

    private string? GetUserId() =>
        User.FindFirstValue(ClaimTypes.NameIdentifier);

    [AllowAnonymous]
    public async Task<IActionResult> Index()
    {
        if (!User.Identity?.IsAuthenticated ?? true)
        {
            // show default landing page for anonymous users
            return View();
        }

        var userId = GetUserId()!;
        var vm = new DashboardViewModel();

        // main checking
        var checking = await _accounts.GetMainCheckingAsync(userId);
        if (checking != null)
        {
            vm.HasChecking = true;
            vm.CheckingBalance = await _accounts.GetBalanceAsync(checking.AccountId, userId);

            // current month tx
            var now = DateTime.Today;
            var monthStart = new DateTime(now.Year, now.Month, 1);
            var nextMonth = monthStart.AddMonths(1).AddDays(-1);

            var tx = await _transactions.GetForPeriodAsync(userId, monthStart, nextMonth);

            vm.MonthIncome = tx.Where(t => t.Amount > 0).Sum(t => t.Amount);
            vm.MonthExpenses = tx.Where(t => t.Amount < 0).Sum(t => t.Amount);

            vm.RecentTransactions = tx
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.TransactionId)
                .Take(5)
                .Select(t => new CheckingTransactionDtos
                {
                    TransactionId = t.TransactionId,
                    CreatedAt = t.CreatedAt,   
                    Amount = t.Amount,
                    CategoryName = null,
                    Memo = t.Description
                })
                .ToList();
        }

        return View(vm);
    }

    [AllowAnonymous]
    public IActionResult About()
    {
        return View();
    }
}

