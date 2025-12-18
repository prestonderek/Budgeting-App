using System.Security.Claims;
using BudgetApp.Logic.Repositories;
using BudgetApp.Models.Budget;
using BudgetApp.Models.ViewModels.Transactions;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace BudgetApp.Controllers
{
    [Authorize]
    public class TransactionsController : Controller
    {
        private readonly ITransactionRepository _transactions;
        private readonly IAccountRepository _accounts;
        private readonly ICategoryRepository _categories;

        public TransactionsController(
            ITransactionRepository transactions,
            IAccountRepository accounts,
            ICategoryRepository categories)
        {
            _transactions = transactions;
            _accounts = accounts;
            _categories = categories;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User ID not found.");

        // GET
        public async Task<IActionResult> Index(DateTime? from, DateTime? to)
        {
            var userId = GetUserId();
            var start = from ?? DateTime.Today.AddMonths(-1);
            var end = to ?? DateTime.Today;

            var tx = await _transactions.GetForPeriodAsync(userId, start, end);
            return View(tx);
        }

        // GET
        public async Task<IActionResult> Create()
        {
            var vm = await BuildEditViewModel(new TransactionEditViewModel());
            return View(vm);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(TransactionEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm = await BuildEditViewModel(vm);
                return View(vm);
            }

            var userId = GetUserId();

            var entity = new Transaction
            {
                UserId = userId,
                AccountId = vm.AccountId,
                CategoryId = vm.CategoryId,
                CreatedAt = vm.CreatedAt,
                Amount = vm.Amount,
                Description = vm.Description
            };

            await _transactions.CreateAsync(entity);
            return RedirectToAction(nameof(Index));
        }

        // GET
        public async Task<IActionResult> Edit(int id)
        {
            var userId = GetUserId();
            var entity = await _transactions.GetByIdAsync(id, userId);
            if (entity == null) return NotFound();

            var vm = new TransactionEditViewModel
            {
                TransactionId = entity.TransactionId,
                AccountId = entity.AccountId,
                CategoryId = entity.CategoryId,
                CreatedAt = entity.CreatedAt,
                Amount = entity.Amount,
                Description = entity.Description
            };

            vm = await BuildEditViewModel(vm);
            return View(vm);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(TransactionEditViewModel vm)
        {
            if (!ModelState.IsValid)
            {
                vm = await BuildEditViewModel(vm);
                return View(vm);
            }

            var userId = GetUserId();
            var entity = await _transactions.GetByIdAsync(vm.TransactionId!.Value, userId);
            if (entity == null) return NotFound();

            entity.AccountId = vm.AccountId;
            entity.CategoryId = vm.CategoryId;
            entity.CreatedAt = vm.CreatedAt;
            entity.Amount = vm.Amount;
            entity.Description = vm.Description;

            await _transactions.UpdateAsync(entity);
            return RedirectToAction(nameof(Index));
        }

        // GET
        public async Task<IActionResult> Delete(int id)
        {
            var userId = GetUserId();
            var entity = await _transactions.GetByIdAsync(id, userId);
            if (entity == null) return NotFound();

            return View(entity);
        }

        // POST
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var userId = GetUserId();
            await _transactions.DeleteAsync(id, userId);
            return RedirectToAction(nameof(Index));
        }

        private async Task<TransactionEditViewModel> BuildEditViewModel(TransactionEditViewModel vm)
        {
            var userId = GetUserId();
            var accounts = await _accounts.GetAllAsync(userId);
            var categories = await _categories.GetAllAsync(userId);

            vm.AccountOptions = accounts.Select(a => new SelectListItem
            {
                Value = a.AccountId.ToString(),
                Text = a.AccountName
            });

            vm.CategoryOptions = categories.Select(c => new SelectListItem
            {
                Value = c.CategoryId.ToString(),
                Text = c.CategoryName
            });

            return vm;
        }
    }
}

