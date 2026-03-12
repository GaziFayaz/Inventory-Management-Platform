using Microsoft.AspNetCore.Identity;

namespace Inventory_Management_Platform.Data.Seeder;

public static class AdminSeeder
{
  public const string AdminRole = "Admin";

  /// <summary>
  /// Ensures the Admin role exists. No user is pre-created here because all
  /// authentication is via external OAuth providers — a pre-created local user
  /// would block the real admin from linking their social account.
  ///
  /// The first admin is bootstrapped automatically: when a user whose email
  /// matches <c>AdminSeed:Email</c> (config / user-secrets) completes OAuth
  /// sign-in for the first time, <see cref="Features.Auth.AuthController"/>
  /// promotes them to the Admin role.
  /// </summary>
  public static async Task SeedAsync(IServiceProvider services)
  {
    var roleManager = services.GetRequiredService<RoleManager<IdentityRole>>();

    if (!await roleManager.RoleExistsAsync(AdminRole))
      await roleManager.CreateAsync(new IdentityRole(AdminRole));
  }
}
