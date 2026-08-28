## 1. Configuration files

- [x] 1.1 Remove `ConnectionStrings:DefaultConnection`, `Jwt:Secret`, `RabbitMq:Uri`, and `SeedAdmin:Password` values from `src/Frozen.API/appsettings.json`, leaving only non-secret defaults and structure (empty strings or the key removed, per key's needs).
- [x] 1.2 Add local-safe secret values for `ConnectionStrings:DefaultConnection`, `Jwt:Secret`, `RabbitMq:Uri`, and `SeedAdmin:Password` to `src/Frozen.API/appsettings.Development.json` (or document them for `dotnet user-secrets` instead if preferred — pick one and be consistent). — Resolved per design.md: `dotnet user-secrets` (see 3.1), `appsettings.Development.json` stays secret-free.
- [x] 1.3 Create `src/Frozen.API/appsettings.Production.json` with non-secret Production-appropriate overrides (e.g. `Serilog:MinimumLevel:Default`, `Cors:AllowedOrigins`), omitting all secret keys entirely. — `Cors:AllowedOrigins` intentionally left out (no real production domain is known); documented in section 4 that it must be set via `Cors__AllowedOrigins__0` env var or edited into this file per deployment.
- [x] 1.4 Confirm `src/Frozen.API/Frozen.API.csproj` includes `appsettings.Production.json` in build output the same way `appsettings.Development.json` already is (ASP.NET Core project SDK does this automatically for `appsettings.*.json`; verify via a local publish). — Verified: `dotnet publish -c Release` output includes `appsettings.Production.json` alongside the other two via the Web SDK's default glob.

## 2. Startup validation

- [x] 2.1 In `src/Frozen.API/Program.cs`, after `builder.Build()`, add a check that runs when `!app.Environment.IsDevelopment()` and verifies `ConnectionStrings:DefaultConnection`, `Jwt:Secret`, and `RabbitMq:Uri` are present and non-empty in `IConfiguration`.
- [x] 2.2 On failure, throw a single exception listing every missing key by name (e.g. `ConnectionStrings:DefaultConnection`, `Jwt:Secret`) so the existing `catch (Exception ex)` / `Log.Fatal` block in `Program.cs` reports it clearly.
- [x] 2.3 Verify the check runs before `db.Database.MigrateAsync()` so a missing connection string fails before attempting a DB connection. — Check is placed immediately after `app.Build()`, before the `using (var scope = ...)` block that runs migrations.

## 3. Local developer setup

- [x] 3.1 Document (in README or a docs file) how to set local secrets via `dotnet user-secrets init` and `dotnet user-secrets set "Jwt:Secret" "..."` (and equivalents for the other three keys) for `src/Frozen.API`. — Added `README.md` "Local development" section.
- [x] 3.2 Document the plain-environment-variable fallback (`ConnectionStrings__DefaultConnection`, `Jwt__Secret`, `RabbitMq__Uri`, `SeedAdmin__Password`) for contributors who prefer not to use user-secrets. — Added `README.md` "Alternative — plain environment variables" subsection.

## 4. Production/deployment setup

- [x] 4.1 Document how to supply the four secret environment variables when running the container (`docker run -e ...` / `--env-file`, or docker-compose `environment:`/`env_file:`), and confirm `ASPNETCORE_ENVIRONMENT=Production` is set (or relies on the ASP.NET Core default) wherever the image is deployed. — Added `README.md` "Production / deployment" section.
- [x] 4.2 Update `Dockerfile` and/or `src/Frozen.API/Dockerfile` comments/README if needed to note that secret env vars are required at runtime, not build time. — Noted in `README.md`; Dockerfiles themselves need no change since they don't reference secrets.

## 5. Verification

- [x] 5.1 Run the API locally with `ASPNETCORE_ENVIRONMENT=Development` and confirm it starts successfully using the values from step 1.2/3.1. — Verified: with secrets supplied via env vars, app logs "Application started" and serves on `http://localhost:5000`; DB migration succeeded against local Postgres.
- [x] 5.2 Run the API locally with `ASPNETCORE_ENVIRONMENT=Production` and no secret env vars set; confirm it fails fast with a clear error naming the missing settings. — Verified: `InvalidOperationException: Missing required configuration for environment 'Production': ConnectionStrings:DefaultConnection, Jwt:Secret, RabbitMq:Uri`, logged via `Log.Fatal` and process exits.
- [x] 5.3 Run the API locally with `ASPNETCORE_ENVIRONMENT=Production` and all four secret env vars set; confirm it starts successfully and connects to the database/RabbitMQ using the env-provided values. — Verified for the database (migration succeeded against local Postgres using the env-provided connection string, past the fail-fast check). RabbitMQ itself is not running in this sandbox so the actual broker connection couldn't be exercised end-to-end; the app correctly used the env-provided `RabbitMq:Uri` and retried against it rather than a hardcoded value.
- [x] 5.4 Confirm no tracked file (`git diff`, `git grep`) contains a working secret value after the change. — Verified via `git diff` / `git grep` below.
