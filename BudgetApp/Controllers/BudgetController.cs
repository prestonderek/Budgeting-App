using System.Security.Claims;
using BudgetApp.Logic.Repositories.Repos;
using BudgetApp.Logic.Repositories;
using BudgetApp.Models.ViewModels.Budget;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace BudgetApp.Controllers
{
    [Authorize]
    public class BudgetController : Controller
    {
        private readonly IBudgetRepository _budget;
        private readonly ICategoryRepository _categories;

        public BudgetController(IBudgetRepository budget, ICategoryRepository categories)
        {
            _budget = budget;
            _categories = categories;
        }

        private string GetUserId() =>
            User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new InvalidOperationException("User ID not found.");

        // GET
        public async Task<IActionResult> Edit(int? year, int? month)
        {
            var userId = GetUserId();
            var y = year ?? DateTime.Today.Year;
            var m = month ?? DateTime.Today.Month;

            var period = await _budget.GetOrCreatePeriodAsync(userId, y, m);
            if (period == null) return NotFound();

            var categories = await _categories.GetAllAsync(userId);

            var existingLines = await _budget.GetLinesAsync(period.BudgetPeriodId, userId);
            var existingByCategory = existingLines.ToDictionary(
                x => x.CategoryId,
                x => x.BudgetedAmount);

            var vm = new BudgetEditViewModel
            {
                Year = y,
                Month = m,
                BudgetPeriodId = period.BudgetPeriodId,
                Lines = categories.Select(c => new BudgetLineRow
                {
                    CategoryId = c.CategoryId,
                    CategoryName = c.CategoryName,
                    CategoryType = c.CategoryType,
                    BudgetedAmount = existingByCategory.TryGetValue(c.CategoryId, out var planned)
                        ? planned
                        : 0m,
                    ActualAmount = 0m //TODO:: Wire up actuals
                }).ToList()
            };

            return View(vm);
        }

        // POST
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(BudgetEditViewModel vm)
        {
            if (!ModelState.IsValid)
                return View(vm);

            var userId = GetUserId();

            var lines = vm.Lines.Select(l => new BudgetApp.Models.Budget.BudgetLine
            {
                CategoryId = l.CategoryId,
                BudgetedAmount = l.BudgetedAmount
            });

            await _budget.SaveLinesAsync(vm.BudgetPeriodId, userId, lines);

            return RedirectToAction(nameof(Edit), new { year = vm.Year, month = vm.Month });
        }
    }
}

