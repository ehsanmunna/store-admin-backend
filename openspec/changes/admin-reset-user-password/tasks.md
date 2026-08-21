## 1. DTOs

- [x] 1.1 Add `ResetPasswordRequest` DTO (e.g. `{ NewPassword }`) in `Frozen.Application/DTOs/Users/`

## 2. Application Layer

- [x] 2.1 Add `ResetPasswordAsync(Guid userId, ResetPasswordRequest request, CancellationToken)` to `IUserService`
- [x] 2.2 Implement `ResetPasswordAsync` in `UserService` using `UserManager.FindByIdAsync`, `UserManager.GeneratePasswordResetTokenAsync`, and `UserManager.ResetPasswordAsync`
- [x] 2.3 Throw a not-found error when the target user id does not exist
- [x] 2.4 Throw `ValidationException` (joining Identity's `IdentityResult.Errors`) when `ResetPasswordAsync` fails validation

## 3. API Layer

- [x] 3.1 Add `POST /api/users/{id}/reset-password` action to `UsersController`, relying on the controller's existing `[Authorize(Roles = UserRoles.Admin)]`
- [x] 3.2 Return 404 when the target user does not exist, 204 (or minimal confirmation) on success

## 4. Verification

- [x] 4.1 Manually verify an `Admin` can reset another existing user's password and log in with the new password
- [x] 4.2 Manually verify a non-admin (`Customer`) caller is rejected (403) when attempting the reset endpoint
- [x] 4.3 Manually verify an unauthenticated caller is rejected (401)
- [x] 4.4 Manually verify resetting a non-existent user id returns 404 without side effects
- [x] 4.5 Manually verify submitting a password that fails Identity's rules returns a validation error and leaves the target user's password unchanged
