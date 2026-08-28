## Why

There is currently no way to reset an existing user's password once it is set — not for self-service, and not for administrators. If a user forgets their password or a credential needs to be rotated, an operator has to reset it directly in the database. Since this backend only has `Admin` and `Customer` roles, and `Admin` is the highest-privilege role (acting as "super admin" for this store), only `Admin` users should be able to reset another existing user's password.

## What Changes

- Add an admin-initiated password reset endpoint: an `Admin` sets a new password directly for an existing user, identified by user id.
- No self-service "forgot password" / email-token flow is introduced by this change.
- Enforce that only users in the `Admin` role can perform a password reset; non-admin callers (including a user resetting their own password this way) are rejected.
- The endpoint accepts a new password meeting the same password rules already enforced by ASP.NET Identity on registration (via `UserManager`).

## Capabilities

### New Capabilities
- `user-management`: Admin-facing management of existing users, starting with the ability for an `Admin` to reset another existing user's password.

### Modified Capabilities
(none — `user-management` has no prior spec)

## Impact

- **API**: New endpoint on `UsersController` (e.g. `POST /api/users/{id}/reset-password`), guarded by the existing `[Authorize(Roles = UserRoles.Admin)]` on the controller.
- **Application/Infrastructure**: New method on `IUserService` / `UserService` (e.g. `ResetPasswordAsync`) using `UserManager<ApplicationUser>` to reset the target user's password (via a reset token generated server-side, since `UserManager` requires one to change a password without knowing the old one).
- **Domain**: No new roles — reuses the existing `UserRoles.Admin` constant.
- No database schema changes expected (ASP.NET Identity already supports password reset tokens).
