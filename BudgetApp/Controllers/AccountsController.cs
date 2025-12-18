using System.Security.Claims;
using BudgetApp.Logic.Repositories;
using BudgetApp.Models.Budget;
using BudgetApp.Models.ViewModels.Accounts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetApp.Controllers
{
    [Authorize]
    public class AccountsController : Controller
    {
        private readonly IAccountRepository _accounts;

        public AccountsController(IAccountRepository accounts)
        {
            _accounts = accounts;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User ID not found.");

        // GET  
        public async Task<IActionResult> Index()
        {
            var userId = GetUserId();
            var accounts = await _accounts.GetAllAsync(userId);

            var vmList = new List<AccountListItemViewModel>();

            foreach (var a in accounts)
            {
                var balance = await _accounts.GetBalanceAsync(a.AccountId, userId);


                vmList.Add(new AccountListItemViewModel
                {
                    AccountId   = a.AccountId,
                    AccountName = a.AccountName,
                    AccountType = a.AccountType,
                    Balance     = balance,
                    IsClosed    = a.IsClosed
                });
            }

            return View(vmList);
        }

        // GET  
        public IActionResult Create()
        {
            return View(new AccountEditViewModel());
        }

        // POST 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(AccountEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var userId = GetUserId();

            var account = new Account
            {
                UserId = userId,
                AccountName = vm.AccountName,
                AccountType = vm.AccountType,
                StartingBalance = vm.StartingBalance,
                IsClosed = false,
                CreatedAt = DateTime.UtcNow
            };

            await _accounts.CreateAsync(account);
            return RedirectToAction(nameof(Index));
        }

        // GET  
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();
            var account = await _accounts.GetByIdAsync(id, userId);
            if (account == null) return NotFound();

            var vm = new AccountEditViewModel
            {
                AccountId = account.AccountId,
                AccountName = account.AccountName,
                AccountType = account.AccountType,
                StartingBalance = account.StartingBalance
            };
            return View(vm);
        }

        // POST 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(AccountEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var userId = GetUserId();
            var account = await _accounts.GetByIdAsync(vm.AccountId!.Value, userId);
            if (account == null) return NotFound();

            account.AccountName = vm.AccountName;
            account.AccountType = vm.AccountType;
            account.StartingBalance = vm.StartingBalance;

            await _accounts.UpdateAsync(account);
            return RedirectToAction(nameof(Index));
        }

        // GET  
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var account = await _accounts.GetByIdAsync(id, userId);
            if (account == null) return NotFound();

            return View(account);   
        }

        // POST 
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetUserId();
            await _accounts.DeleteAsync(id, userId);
            return RedirectToAction(nameof(Index));
        }
    }
}

