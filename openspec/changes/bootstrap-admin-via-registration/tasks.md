## 1. Remove startup admin seeding

- [x] 1.1 Remove the `AdminSeeder.SeedAsync` call from `Frozen.API/Program.cs`
- [x] 1.2 Delete `Frozen.Infrastructure/Identity/AdminSeeder.cs`
- [x] 1.3 Delete `Frozen.Infrastructure/Identity/SeedAdminSettings.cs`
- [x] 1.4 Remove the `SeedAdmin` configuration section from `Frozen.API/appsettings.json` and any environment-specific appsettings files
- [x] 1.5 Remove the `AdminSeeder`/`SeedAdminSettings` DI registrations, if any, from startup/service registration code

## 2. Bootstrap-first-admin registration logic

- [x] 2.1 In `Frozen.Infrastructure/Identity/IdentityService.RegisterAsync`, check whether any user currently holds the `Admin` role
- [x] 2.2 Assign the `Admin` role to the new user when no Admin exists yet; otherwise assign `Customer` as today
- [ ] 2.3 Add/update backend tests covering: first registration becomes Admin, second registration becomes Customer, duplicate email is still rejected — skipped: no backend test project exists in the solution; deferred as a follow-up rather than expanding this change's scope

## 3. Frontend registration page

- [x] 3.1 Add `frontend/src/app/features/auth/register/register.component.ts` (+ template) modeled on `login/login.component.ts`, with fields for first name, last name, email, and password
- [x] 3.2 Wire the new component into the router alongside the existing `login` route, publicly accessible (no auth guard)
- [x] 3.3 On submit, call `auth.service.ts`'s existing `register()` method
- [x] 3.4 On success, store the returned session (token/roles) and navigate into the app the same way `login.component.ts` does on successful login
- [x] 3.5 On failure (e.g. duplicate email), display the error on the registration page without navigating away
- [x] 3.6 Add a link between the login and registration pages so users can navigate between them

## 4. Verification

- [ ] 4.1 Run backend tests — skipped: no backend test project exists in the solution
- [x] 4.2 Manually verify: starting the app against a fresh database creates no user; registering the first account grants Admin and can access admin-only endpoints/pages; registering a second account grants Customer — verified live against local dev DB (Postgres): backend and frontend both build cleanly; registering with no existing Admin returns `roles:["Admin"]`; registering while an Admin exists returns `roles:["Customer"]`; duplicate email is rejected with 400. Test data was cleaned up and the pre-existing admin's role was restored afterward.
- [x] 4.3 Update `backend/README.md` (or equivalent onboarding docs) if it currently documents the seeded admin credentials, replacing that guidance with "register the first account after first launch"
