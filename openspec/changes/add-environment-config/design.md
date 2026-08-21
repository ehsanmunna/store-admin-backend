## Context

The API is a .NET 8 ASP.NET Core app (`src/Frozen.API`) with `Frozen.Infrastructure/DependencyInjection.cs` wiring `ConnectionStrings:DefaultConnection`, `Jwt:*` (`JwtSettings`), `RabbitMq:*` (`RabbitMqSettings`), and `SeedAdmin:*` (`SeedAdminSettings`) from `IConfiguration` via `services.Configure<T>(configuration.GetSection(...))`. Today all four are hardcoded, working values committed in `src/Frozen.API/appsettings.json`; `appsettings.Development.json` only overrides `Logging`. Both Dockerfiles publish a Release build and set `ASPNETCORE_URLS`, but do not set `ASPNETCORE_ENVIRONMENT`, so containers currently default to `Production` per ASP.NET Core convention while still loading the committed secrets from `appsettings.json`. See `proposal.md` - Why for the underlying problem.

## Goals / Non-Goals

**Goals:**
- Use ASP.NET Core's built-in configuration layering (`appsettings.json` → `appsettings.{Environment}.json` → environment variables) so no code change is needed to switch environments, only `ASPNETCORE_ENVIRONMENT` and env vars.
- Make secret-shaped settings (`ConnectionStrings:DefaultConnection`, `Jwt:Secret`, `RabbitMq:Uri`, `SeedAdmin:Password`) absent from every tracked file.
- Fail startup loudly and specifically when a required secret is missing outside Development, instead of connecting with an empty string or throwing a deep, unrelated exception later.

**Non-Goals:**
- No secrets manager integration (Azure Key Vault, AWS Secrets Manager, etc.) — plain environment variables are sufficient for this change; a secrets manager can be layered in later without changing the spec.
- No rotation of the currently-committed secret values (DB password, JWT secret, RabbitMQ credentials) — tracked separately as an operational follow-up since git history retains them regardless of this change.
- No new configuration UI or admin endpoint for managing settings.

## Decisions

**Use `dotnet user-secrets` for local development, not a `.env` file.** The user selected the ASP.NET Core standard approach. `dotnet user-secrets` stores values outside the repo (under the user profile), is built into the SDK, and is the idiomatic .NET equivalent of a local `.env` file — no new package required. Plain local environment variables remain a documented fallback for contributors who prefer them.

**Validate required settings with a small startup check in `Program.cs`, not `[Required]` data annotations on the options classes.** `IOptions<T>` validation via `ValidateDataAnnotations()` only throws lazily, the first time the options are resolved (e.g., on the first HTTP request needing JWT validation), which is a confusing failure mode for a missing DB connection string that's needed immediately for migrations. Instead, add an explicit check right after `builder.Build()` (or right before `db.Database.MigrateAsync()`) that inspects the four secret-backed settings directly from `IConfiguration`/bound options and throws a clear, single exception listing every missing key when `app.Environment.IsProduction()` (extendable to other non-Development environments later). This keeps the fail-fast behavior in one obvious place near existing startup code (`Program.cs:80-87`).

**Keep `appsettings.json` as the schema/defaults file, add `appsettings.Production.json` for non-secret Production overrides.** Mirrors the existing `appsettings.Development.json` pattern already in the repo. Non-secret Production-appropriate values (e.g., `Serilog:MinimumLevel:Default`, `Cors:AllowedOrigins`) live here; secret keys are omitted entirely (not even placeholders) so their absence is unambiguous and the fail-fast check has nothing to accidentally treat as "present."

**Env var mapping relies on ASP.NET Core's default double-underscore convention** (`ConnectionStrings__DefaultConnection`, `Jwt__Secret`, `RabbitMq__Uri`, `SeedAdmin__Password`) — already active today via `builder.Configuration` default sources, so no `Program.cs` change is needed to enable it, only documentation.

## Risks / Trade-offs

- [Startup validation only checks Production] → Anyone running a third custom environment name (e.g. `Staging`) gets no fail-fast protection. Mitigated by checking `!app.Environment.IsDevelopment()` instead of `IsProduction()` specifically, so any non-Development environment is covered.
- [Committed secrets remain in git history] → Rotating the DB password, JWT secret, and RabbitMQ credentials is required for this change to be a real security fix, not just a structural one. Out of scope for this change's code diff; call out explicitly in tasks.md as a manual operational step for the user.
- [Docker images don't set `ASPNETCORE_ENVIRONMENT`] → Relying on the ASP.NET Core default (`Production`) is correct but implicit; documenting it explicitly in the Dockerfile/README avoids confusion for anyone who assumes Development is the default.

## Migration Plan

1. Add `appsettings.Production.json`, strip secret keys from `appsettings.json` and `appsettings.Development.json`.
2. Add the startup fail-fast check in `Program.cs`.
3. Set local secrets via `dotnet user-secrets` for the current developer's machine (manual, not a code change).
4. Update deployment configuration (docker-compose / hosting platform env vars) to supply the four secret env vars before deploying this change — deploying without doing so will now cause the container to fail to start, which is the intended behavior change.
5. Rollback: revert the code changes; the app returns to reading secrets from the committed `appsettings.json` (not recommended, but restores prior behavior if the environment-variable rollout needs more time).
