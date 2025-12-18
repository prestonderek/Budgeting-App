using BudgetApp.Data;
using BudgetApp.Models.ViewModels.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ApplicationDbContext _db;

        public AdminController(UserManager<IdentityUser> userManager, ApplicationDbContext db)
        {
            _userManager = userManager;
            _db = db;
        }

        public async Task<IActionResult> Index()
        {
            var users = await _userManager.Users.ToListAsync();

            var vm = new AdminDashboardViewModel
            {
                TotalUsers = users.Count,
                TotalAccounts = await _db.Accounts.CountAsync(),
                TotalTransactions = await _db.Transactions.CountAsync(),
                Users = new List<AdminUserItem>()
            };

            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);

                vm.Users.Add(new AdminUserItem
                {
                    UserId = u.Id,
                    Email = u.Email ?? u.UserName ?? "(no email)",
                    Roles = roles
                });
            }

            return View(vm);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> MakeAdmin(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
                return NotFound();

            if (!await _userManager.IsInRoleAsync(user, "Admin"))
            {
                await _userManager.AddToRoleAsync(user, "Admin");
            }

            return RedirectToAction(nameof(Index));
        }
    }
}

