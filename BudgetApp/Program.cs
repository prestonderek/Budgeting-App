using BudgetApp.Config;
using BudgetApp.Data;
using BudgetApp.Logic.Repositories;
using BudgetApp.Logic.Repositories.Repos;
using BudgetApp.Logic.Repositories.CachedRepos;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.Extensions.Caching.Memory;
using BudgetApp.Logic.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.Configure<BudgetSettings>(builder.Configuration.GetSection("BudgetSettings"));

builder.Services.Configure<PlaidSettings>(builder.Configuration.GetSection("Plaid"));

builder.Services.AddHttpClient<IPlaidService, PlaidService>();

var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");

builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

builder.Services.AddMemoryCache();

builder.Services.AddScoped<AccountRepo>();
builder.Services.AddScoped<CategoryRepo>();
builder.Services.AddScoped<TransactionRepo>();
builder.Services.AddScoped<BudgetRepo>();
builder.Services.AddScoped<IPlaidService, PlaidService>();

builder.Services.AddScoped<IAccountRepository>(sp =>
{
    var repo = sp.GetRequiredService<AccountRepo>();
    var cache = sp.GetRequiredService<IMemoryCache>();
    return new CachedAccountRepo(repo, cache);
});

builder.Services.AddScoped<ICategoryRepository>(sp =>
{
    var repo = sp.GetRequiredService<CategoryRepo>();
    var cache = sp.GetRequiredService<IMemoryCache>();
    return new CachedCategoryRepo(repo, cache);
});

builder.Services.AddScoped<ITransactionRepository>(sp =>
    sp.GetRequiredService<TransactionRepo>());

builder.Services.AddScoped<IBudgetRepository>(sp =>
    sp.GetRequiredService<BudgetRepo>());

builder.Services.AddDatabaseDeveloperPageExceptionFilter();

builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
    .AddRoles<IdentityRole>()
    .AddEntityFrameworkStores<ApplicationDbContext>();

builder.Services.AddControllersWithViews();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    await SeedRolesAndAdminAsync(services);
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseMigrationsEndPoint();
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/Home/Error");
    // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();

app.UseAuthentication();
app.UseAuthorization();

app.MapStaticAssets();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}")
    .WithStaticAssets();

app.MapRazorPages()
   .WithStaticAssets();

await app.RunAsync();

//Seed default roles and an admin user
static async Task SeedRolesAndAdminAsync(IServiceProvider services)
{
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<IdentityUser>>();

    const string adminRoleName = "Admin";

    if (!await roleManager.RoleExistsAsync(adminRoleName))
    {
        await roleManager.CreateAsync(new IdentityRole(adminRoleName));
    }

    const string adminEmail = "admin@example.com";
    const string adminPassword = "Admin123!"; 

    var adminUser = await userManager.FindByEmailAsync(adminEmail);
    if (adminUser == null)
    {
        adminUser = new IdentityUser
        {
            UserName = adminEmail,
            Email = adminEmail,
            EmailConfirmed = true
        };

        var result = await userManager.CreateAsync(adminUser, adminPassword);
        if (!result.Succeeded)
        {
            return;
        }
    }

    if (!await userManager.IsInRoleAsync(adminUser, adminRoleName))
    {
        await userManager.AddToRoleAsync(adminUser, adminRoleName);
    }
}
