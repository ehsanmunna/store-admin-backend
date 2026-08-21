## Context

`UsersController` already exposes `GET /api/users` and `POST /api/users` (create admin), both guarded by `[Authorize(Roles = UserRoles.Admin)]` at the controller level. `IUserService`/`UserService` wrap `UserManager<ApplicationUser>` (ASP.NET Identity). There are only two roles today, `Admin` and `Customer` (`UserRoles` in `Frozen.Domain.Enums`); per proposal.md, `Admin` is treated as the "super admin" for this backend — no new role is introduced. See proposal.md for motivation; see specs/user-management/spec.md for requirements.

## Goals / Non-Goals

**Goals:**
- Let an `Admin` set a new password directly for any existing user by id.
- Reuse the existing controller-level `Admin` authorization already applied to `UsersController`.

**Non-Goals:**
- No email/token-based "forgot password" self-service flow.
- No new role (`SuperAdmin`) — `Admin` is authoritative.
- No change to login, registration, or token issuance behavior.

## Decisions

- **Endpoint shape**: `POST /api/users/{id}/reset-password` with body `{ newPassword }`, added to the existing `UsersController`. Reuses the controller's existing `[Authorize(Roles = UserRoles.Admin)]`, so no new authorization plumbing is needed. Alternative considered: a separate `AdminController`/route group — rejected as unnecessary since user management already lives in `UsersController`.
- **Password change mechanism**: `UserManager<ApplicationUser>` has no "set password directly" API without knowing the old one. Use `UserManager.GeneratePasswordResetTokenAsync(user)` followed immediately by `UserManager.ResetPasswordAsync(user, token, newPassword)` inside the same request — the token is generated and consumed server-side in one call, never exposed to the client. This reuses Identity's built-in password-validation pipeline (`IPasswordValidator`) so the same complexity rules as registration apply automatically.
- **Response**: Return `204 No Content` (or a minimal confirmation) — no password or token is echoed back, consistent with not introducing a token-based flow.
- **Error mapping**: User-not-found -> 404. Identity `ResetPasswordAsync` failures (e.g., password too weak) -> existing `ValidationException` pattern already used elsewhere in `UserService`/`IdentityService`, surfaced the same way as other validation errors in this codebase.

## Risks / Trade-offs

- [Any `Admin` can reset any other `Admin`'s password, including their own target of privilege escalation between admins] → Accepted per the "reuse existing Admin role" decision; there is no higher role to restrict to. Out of scope to introduce one.
- [Generating and immediately consuming a reset token in the same request is slightly unusual compared to typical two-step reset flows] → Acceptable since this is an internal admin action, not a user-facing email flow; avoids adding token storage/expiry/email infrastructure for a feature that doesn't need it.
