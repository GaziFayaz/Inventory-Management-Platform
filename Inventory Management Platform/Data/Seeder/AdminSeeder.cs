using Inventory_Management_Platform.Models;
using Microsoft.AspNetCore.Identity;

namespace Inventory_Management_Platform.Data.Seeder;

public static class AdminSeeder
{
  public const string AdminRole = "Admin";

  /// <summary>
  /// Seeds the Admin role and a default admin user if they don't already exist.
  /// Call this once at startup after app.Build().
  /// Credentials should come from configuration — never hardcode in production.
  /// </summary>
  public static async Task SeedAsync(IServiceProvider services)
  {
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();
    var userManager = services.GetRequiredService<UserManager<AppUser>>();
    var config = services.GetRequiredService<IConfiguration>();

    // ── Seed Admin role ───────────────────────────────────────────────────
    if (!await roleManager.RoleExistsAsync(AdminRole))
    {
      await roleManager.CreateAsync(new IdentityRole(AdminRole));
    }

    // ── Seed Admin user ───────────────────────────────────────────────────
    var adminEmail = config["AdminSeed:Email"] ?? "admin@example.com";
    var adminPassword = config["AdminSeed:Password"]
        ?? throw new InvalidOperationException(
            "AdminSeed:Password is not configured. Set it in user secrets or environment variables.");

    var existing = await userManager.FindByEmailAsync(adminEmail);
    if (existing is null)
    {
      var admin = new AppUser
      {
        UserName = "admin",
        Email = adminEmail,
        DisplayName = "Administrator",
        EmailConfirmed = true
      };

      var result = await userManager.CreateAsync(admin, adminPassword);
      if (!result.Succeeded)
      {
        var errors = string.Join(", ", result.Errors.Select(e => e.Description));
        throw new InvalidOperationException($"Failed to create admin user: {errors}");
      }

      await userManager.AddToRoleAsync(admin, AdminRole);
    }
  }
}
