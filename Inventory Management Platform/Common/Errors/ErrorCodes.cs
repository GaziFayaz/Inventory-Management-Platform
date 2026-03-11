namespace Inventory_Management_Platform.Common.Errors;

/// <summary>
/// Central registry of all error code strings used as API response
/// <c>errorCode</c> fields and as frontend i18n lookup keys.
///
/// Naming convention: &lt;resource&gt;.&lt;condition&gt;
/// </summary>
public static class ErrorCodes
{
    // ── Generic ──────────────────────────────────────────────────────────────
    public const string ServerError        = "server_error";
    public const string Fallback           = "error";

    // ── Auth ─────────────────────────────────────────────────────────────────
    public const string Unauthorized       = "auth.unauthorized";
    public const string Forbidden          = "auth.forbidden";
    public const string Blocked            = "auth.blocked";

    // ── Conflict / Concurrency ───────────────────────────────────────────────
    public const string OptimisticLock     = "conflict.optimistic_lock";
    public const string CustomIdDuplicate  = "item.custom_id_duplicate";

    // ── Auth / Social ────────────────────────────────────────────────────────
    public const string AuthProviderFailed = "auth.provider_failed";

    // ── User ─────────────────────────────────────────────────────────────────
    public const string UserNotFound       = "user.not_found";

    // ── Admin ────────────────────────────────────────────────────────────────
    public const string CannotDeleteSelf   = "admin.cannot_delete_self";
}
