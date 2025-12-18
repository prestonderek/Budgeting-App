using System.Security.Claims;
using BudgetApp.Logic.Repositories;
using BudgetApp.Models.Api;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CheckingApiController : ControllerBase
    {
        private readonly IAccountRepository _accounts;
        private readonly ITransactionRepository _transactions;

        public CheckingApiController(
            IAccountRepository accounts,
            ITransactionRepository transactions)
        {
            _accounts = accounts;
            _transactions = transactions;
        }

        private string GetUserId()
        {
            return User.FindFirstValue(ClaimTypes.NameIdentifier)
                   ?? throw new InvalidOperationException("User ID not found.");
        }

        // GET
        [HttpGet]
        public async Task<ActionResult<CheckingSummaryDto>> GetMainChecking()
        {
            var userId = GetUserId();

            var account = await _accounts.GetMainCheckingAsync(userId);
            if (account == null)
                return NotFound("No main checking account found for this user.");

            var balance = await _accounts.GetBalanceAsync(account.AccountId, userId);

            // last 10 transactions for this account
            var tenDaysAgo = DateTime.UtcNow.AddMonths(-1);
            var tx = await _transactions.GetForPeriodAsync(userId, tenDaysAgo, DateTime.UtcNow);

            var recentForAccount = tx
                .Where(t => t.AccountId == account.AccountId)
                .OrderByDescending(t => t.CreatedAt)
                .ThenByDescending(t => t.TransactionId)
                .Take(10)
                .ToList();

            var dto = new CheckingSummaryDto
            {
                AccountId = account.AccountId,
                AccountName = account.AccountName,
                Balance = balance,
                RecentTransactions = recentForAccount.Select(t => new CheckingTransactionDtos
                {
                    TransactionId = t.TransactionId,
                    CreatedAt = t.CreatedAt,
                    Amount = t.Amount,
                    CategoryName = null,
                    Memo = t.Description
                }).ToList()
            };

            return Ok(dto);
        }
    }
}

