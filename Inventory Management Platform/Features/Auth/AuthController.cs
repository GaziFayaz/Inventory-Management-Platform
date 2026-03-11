using Inventory_Management_Platform.Common;
using Inventory_Management_Platform.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace Inventory_Management_Platform.Features.Auth;

[ApiController]
[Route("auth")]
public sealed class AuthController(
    SignInManager<AppUser> signInManager,
    IAuthService           authService) : ControllerBase
{
  [HttpGet("login/{provider}")]
  [AllowAnonymous]
  public IActionResult Login(string provider)
  {
    var properties = authService.BuildLoginProperties(
        provider,
        callbackUrl: Url.Action(nameof(ExternalCallback), "Auth")!);

    return Challenge(properties, provider);
  }

  // ── GET /auth/external-callback ─────────────────────────────────────────
  // Called by the OAuth middleware after it validates the external token.
  // Finds or creates the AppUser then drops an Identity cookie.
  [HttpGet("external-callback")]
  [AllowAnonymous]
  public async Task<IActionResult> ExternalCallback()
  {
    var redirectUrl = await authService.ProcessExternalCallbackAsync();
    return Redirect(redirectUrl);
  }

  // ── POST /auth/logout ───────────────────────────────────────────────────
  [HttpPost("logout")]
  [Authorize(Policy = "Authenticated")]
  public async Task<IActionResult> Logout()
  {
    await signInManager.SignOutAsync();
    return Ok(ApiResponse.Ok<object>(null!));
  }

  // ── GET /auth/me ────────────────────────────────────────────────────────
  [HttpGet("me")]
  [Authorize(Policy = "Authenticated")]
  public async Task<IActionResult> Me()
  {
    var dto = await authService.GetCurrentUserDtoAsync(User);
    return Ok(ApiResponse.Ok(dto));
  }

  // ── Helpers ─────────────────────────────────────────────────────────────
}
