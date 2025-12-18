using BudgetApp.Data;
using BudgetApp.Logic.Services;
using BudgetApp.Models.Budget;
using BudgetApp.Models.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace BudgetApp.Controllers
{
    [Authorize]
    public class BankController : Controller
    {
        private readonly IPlaidService _plaid;
        private readonly ApplicationDbContext _db;

        public BankController(IPlaidService plaid, ApplicationDbContext db)
        {
            _plaid = plaid;
            _db = db;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User ID not found.");

        // GET
        public async Task<IActionResult> Connections()
        {
            var userId = GetUserId();

            var links = await _db.BankLinks
                .Where(b => b.UserId == userId && b.IsActive)
                .ToListAsync();

            return View(links);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Sync(int id)
        {
            var userId = GetUserId();
            await _plaid.SyncAccountsAndTransactionsAsync(userId, id);
            return RedirectToAction(nameof(Connections));
        }


        // GET
        [HttpGet]
        public async Task<IActionResult> CreateLinkToken()
        {
            var userId = GetUserId();
            var linkToken = await _plaid.CreateLinkTokenAsync(userId);
            return Json(new { link_token = linkToken });
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ExchangePublicToken([FromForm] string public_token)
        {
            var userId = GetUserId();

            var (accessToken, itemId) = await _plaid.ExchangePublicTokenAsync(public_token);

            var bankLink = new BankLink
            {
                UserId = userId,
                Provider = "Plaid",
                ItemId = itemId,
                AccessToken = accessToken,
                InstitutionName = null
            };

            _db.BankLinks.Add(bankLink);
            await _db.SaveChangesAsync();

            // TODO: call SyncAccountsAndTransactionsAsync(userId, bankLink.BankLinkId)

            return RedirectToAction("Connections");
        }
    }
}

