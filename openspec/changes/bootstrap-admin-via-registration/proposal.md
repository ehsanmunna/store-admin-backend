## Why

The system currently creates a default admin account (`admin@frozen.local`) automatically on every server startup, and the public registration page does not exist yet in the frontend. Auto-seeding a well-known admin account is undesirable (it ships a predictable credential and runs unconditionally on every deploy/launch), and there is currently no way for an operator to create the first admin account except through this seed. Instead, the first admin account should be created explicitly, through the same registration flow regular users use.

## What Changes

- **BREAKING**: Remove the automatic admin-seeding behavior that runs on every application startup (`AdminSeeder.SeedAsync` call in `Program.cs`, plus the `AdminSeeder` class and `SeedAdmin` configuration section). No account is created automatically anymore.
- Change `POST api/auth/register` so that when no user currently holds the `Admin` role, the newly registered account is assigned the `Admin` role instead of `Customer` (bootstrap-first-admin). Once at least one `Admin` exists, all subsequent registrations continue to be assigned `Customer` as today.
- Add a public registration page to the Angular admin frontend (`frontend/src/app/features/auth/register/`) that calls the existing (currently unused) `register()` method on `auth.service.ts`, with a route wired up alongside the existing `login/` feature.
- On successful registration, behave like login: store the returned auth token/roles and redirect into the admin app (mirroring `login.component.ts`'s handling of the auth response).

## Capabilities

### New Capabilities
- `user-registration`: Public user registration, including the bootstrap rule that grants the `Admin` role to the very first registered account and `Customer` to every account after that, and the corresponding registration UI.

### Modified Capabilities
(none — no existing archived specs cover registration or startup seeding)

## Impact

- Backend: `Frozen.API/Program.cs` (remove seeder invocation), `Frozen.Infrastructure/Identity/AdminSeeder.cs` (remove), `Frozen.Infrastructure/Identity/SeedAdminSettings.cs` (remove), `Frozen.API/appsettings.json` (remove `SeedAdmin` section), `Frozen.Infrastructure/Identity/IdentityService.cs` (`RegisterAsync` role assignment logic), `backend/tools/CreateAdmin` (verify whether this console tool still makes sense as a manual fallback for creating additional admins later, or should also be reviewed).
- Frontend: new `frontend/src/app/features/auth/register/` component + route, `auth.service.ts` (already has `register()`), `core/models/auth.model.ts` if the register request/response shape needs adjusting.
- Deployment: environments that relied on the seeded `admin@frozen.local` account for first access must instead register the first account through the app after this change ships.
