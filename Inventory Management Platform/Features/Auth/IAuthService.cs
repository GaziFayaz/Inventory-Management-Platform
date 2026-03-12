using System.Security.Claims;
using Inventory_Management_Platform.Contracts.Auth;
using Inventory_Management_Platform.Models;
using Microsoft.AspNetCore.Authentication;

namespace Inventory_Management_Platform.Features.Auth;

public interface IAuthService
{
    /// <summary>
    /// Validates the provider name and builds the OAuth challenge properties.
    /// </summary>
    /// <exception cref="Common.Errors.AppException">Thrown when the provider is not supported.</exception>
    AuthenticationProperties BuildLoginProperties(string provider, string callbackUrl);

    /// <summary>
    /// Processes the OAuth external-login callback. Finds or creates the
    /// <see cref="AppUser"/>, links the external login, and signs the user in.
    /// </summary>
    /// <returns>The URL the browser should be redirected to after the flow.</returns>
    Task<string> ProcessExternalCallbackAsync();

    /// <summary>Builds a <see cref="UserDto"/> for the currently signed-in principal.</summary>
    Task<UserDto> GetCurrentUserDtoAsync(ClaimsPrincipal principal);

    /// <summary>Builds a <see cref="UserDto"/> from a loaded <see cref="AppUser"/>.</summary>
    Task<UserDto> BuildUserDtoAsync(AppUser user);
}
