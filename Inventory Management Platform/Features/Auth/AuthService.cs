using System.Security.Claims;
using Inventory_Management_Platform.Common.Errors;
using Inventory_Management_Platform.Contracts.Auth;
using Inventory_Management_Platform.Models;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Identity;

namespace Inventory_Management_Platform.Features.Auth;

public sealed class AuthService(
    UserManager<AppUser> userManager,
    SignInManager<AppUser> signInManager,
    IConfiguration configuration) : IAuthService
{
    private static readonly string[] SupportedProviders = [
        "Google", "Facebook"
    ];

    public AuthenticationProperties BuildLoginProperties(string provider, string callbackUrl)
    {
        if (!SupportedProviders.Contains(provider))
            throw new AppException(400, $"Unsupported provider '{provider}'.", ErrorCodes.Fallback);

        var properties = signInManager.ConfigureExternalAuthenticationProperties(provider, callbackUrl);

        if (provider == "Google")
            properties.SetParameter("prompt", "select_account");

        return properties;
    }
    public async Task<string> ProcessExternalCallbackAsync()
    {
        var frontendUrl = configuration["FrontendUrl"] ?? "/";

        var info = await signInManager.GetExternalLoginInfoAsync();
        if (info is null)
            return $"{frontendUrl}?error={ErrorCodes.AuthProviderFailed}";

        // 1. Try signing in with an existing external login link.
        var result = await signInManager.ExternalLoginSignInAsync(
            info.LoginProvider,
            info.ProviderKey,
            isPersistent: true,
            bypassTwoFactor: true);

        if (result.Succeeded)
            return frontendUrl;

        if (result.IsLockedOut || result.IsNotAllowed)
            return $"{frontendUrl}?error={ErrorCodes.Blocked}";

        // 2. No existing link — create a new AppUser and link the provider.
        var email = info.Principal.FindFirstValue(ClaimTypes.Email);
        if (string.IsNullOrWhiteSpace(email))
            return $"{frontendUrl}?error={ErrorCodes.AuthProviderFailed}";

        var displayName = info.Principal.FindFirstValue(ClaimTypes.Name) ?? email[..email.IndexOf('@')];

        var user = new AppUser
        {
            UserName = email,
            Email = email,
            DisplayName = displayName,
            CreatedAt = DateTime.UtcNow
        };

        var createResult = await userManager.CreateAsync(user);
        if (!createResult.Succeeded)
            return $"{frontendUrl}?error={ErrorCodes.AuthProviderFailed}";

        var linkResult = await userManager.AddLoginAsync(user, info);
        if (!linkResult.Succeeded)
            return $"{frontendUrl}?error={ErrorCodes.AuthProviderFailed}";

        // Auto-promote to Admin if this email is designated as the bootstrap admin.
        var adminEmail = configuration["AdminSeed:Email"];
        if (!string.IsNullOrWhiteSpace(adminEmail) &&
            string.Equals(email, adminEmail, StringComparison.OrdinalIgnoreCase))
        {
            await userManager.AddToRoleAsync(user, "Admin");
        }

        await signInManager.SignInAsync(user, isPersistent: true);
        return frontendUrl;
    }

    public async Task<UserDto> GetCurrentUserDtoAsync(ClaimsPrincipal principal)
    {
        var user = await userManager.GetUserAsync(principal);
        if (user is null)
            throw new AppException(404, "User not found.", ErrorCodes.UserNotFound);

        return await BuildUserDtoAsync(user);
    }

    public async Task<UserDto> BuildUserDtoAsync(AppUser user)
    {
        var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
        return new UserDto(
            user.Id,
            user.DisplayName,
            user.Email!,
            isAdmin,
            user.IsBlocked,
            user.CreatedAt
        );
    }
}
