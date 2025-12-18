using BudgetApp.Models.Budget;
using BudgetApp.Models.Services;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace BudgetApp.Data;

public class ApplicationDbContext : IdentityDbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<Account> Accounts { get; set; } = default!;
    public DbSet<Category> Categories { get; set; } = default!;
    public DbSet<Transaction> Transactions { get; set; } = default!;
    public DbSet<BudgetPeriod> BudgetPeriods { get; set; } = default!;
    public DbSet<BudgetLine> BudgetLines { get; set; } = default!;
    public DbSet<BankLink> BankLinks { get; set; } = default!;

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        builder.Entity<Account>()
            .Property(a => a.StartingBalance)
            .HasColumnType("decimal(18,2)");

        builder.Entity<Transaction>()
            .Property(t => t.Amount)
            .HasColumnType("decimal(18,2)");

        builder.Entity<BudgetLine>()
            .Property(b => b.BudgetedAmount)
            .HasColumnType("decimal(18,2)");
    }

}
