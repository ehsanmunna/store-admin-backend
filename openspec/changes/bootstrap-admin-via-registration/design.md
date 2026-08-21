## Context

See proposal.md - Why. Today `Program.cs` calls `AdminSeeder.SeedAsync` on every startup, which checks whether any `ApplicationUser` holds the `Admin` role and, if not, creates/promotes one from `SeedAdminSettings` (bound from the `SeedAdmin` config section). Separately, `IdentityService.RegisterAsync` always assigns the `Customer` role, and the Angular frontend has no registration route (`register()` exists on `auth.service.ts` but is unused).

## Goals / Non-Goals

**Goals:**
- Move the "who is the first admin" decision out of startup configuration and into the registration flow itself.
- Keep the change self-contained to registration/startup — do not alter how roles are enforced elsewhere (`[Authorize(Roles = ...)]` usage is unchanged).

**Non-Goals:**
- Multi-admin invite/approval flows (e.g. requiring an existing admin to approve new admins) — out of scope; only the *first* account is special-cased.
- Changing how `UsersController`'s existing admin-only `CreateAdminAsync` path works — it remains available for an already-authenticated admin to create more admins.

## Decisions

- **Bootstrap check location**: Perform the "does any Admin exist?" check inside `IdentityService.RegisterAsync`, immediately before assigning a role, using `RoleManager`/`UserManager` (e.g. `await userManager.GetUsersInRoleAsync(UserRoles.Admin)` count, or `roleManager` + a users-in-role query) — the same check `AdminSeeder` already performs today, just relocated. Alternative considered: a dedicated `IAdminBootstrapService` — rejected as unnecessary indirection for a single conditional.
- **Removal, not feature flag**: Delete `AdminSeeder`, `SeedAdminSettings`, the `Program.cs` invocation, and the `SeedAdmin` appsettings section outright rather than gating them behind a flag. The user explicitly does not want seeding on launch; keeping dead/disabled seed code adds no value. Alternative considered: keep `AdminSeeder` but default it to disabled — rejected, since nothing would call it and it would just be dead code.
- **Race on concurrent first registrations**: Two simultaneous registration requests could both observe zero admins and both be assigned `Admin`. This is accepted as a low-severity, low-likelihood risk (first-launch bootstrap is a one-time, low-concurrency event, and ending up with two initial admins instead of one is not a security regression compared to today's single seeded admin). No additional locking/transaction is introduced for this.
- **`backend/tools/CreateAdmin` console tool**: Left untouched by this change. It remains available as a manual, out-of-band way to create an admin account (e.g. for disaster recovery if all admins are locked out), independent of the registration bootstrap rule.
- **Frontend registration page**: Mirror the structure and auth-response handling of the existing `login/` feature (`frontend/src/app/features/auth/login/`) rather than introducing a new pattern, so the new `register/` feature reuses the same `auth.service.ts` session-storage/navigation behavior.

## Risks / Trade-offs

- [Anyone who can reach the registration endpoint before the first admin registers can claim the Admin role] → Accepted: this mirrors the trust assumption of "whoever deploys the app registers first," which is a stated goal (replacing the seeded credential with an explicit first-registration step). Operators should register the admin account immediately after deploy, before exposing the app publicly.
- [Environments that already have a seeded `admin@frozen.local` from before this change] → No action needed: an Admin already exists in those databases, so the bootstrap rule simply never triggers there; new registrations continue to default to Customer as expected.
- [Losing the seeded recovery account] → Mitigated by leaving `backend/tools/CreateAdmin` in place as a manual fallback for creating additional/replacement admins.

## Migration Plan

1. Remove `AdminSeeder.SeedAsync` call from `Program.cs`, then delete `AdminSeeder.cs` and `SeedAdminSettings.cs`, then remove the `SeedAdmin` section from `appsettings.json` (and any environment-specific appsettings overrides).
2. Update `IdentityService.RegisterAsync` to assign `Admin` when no Admin exists yet, else `Customer`.
3. Add the Angular `register/` feature + route, reusing `auth.service.ts`'s `register()` and the login flow's session-handling.
4. No database migration is required — no schema changes.
5. Rollback: revert the commits; environments without any Admin yet would need the seeder (or `backend/tools/CreateAdmin`) reinstated temporarily if rolled back after already running without one.
