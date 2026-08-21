## Why

`appsettings.json` currently hardcodes real-looking secrets (DB password, JWT signing secret, RabbitMQ credentials, admin seed password) and is committed to git, so local development and production are forced to share the same values. There is no way to point the API at a different database, message broker, or secret set per environment without editing a tracked file.

## What Changes

- Split configuration by ASP.NET Core environment: `appsettings.json` keeps only non-secret defaults/structure, `appsettings.Development.json` holds safe local-dev values, and a new `appsettings.Production.json` holds production-safe non-secret overrides (e.g. log level, CORS origins) — no secrets in either tracked file.
- **BREAKING**: Remove the hardcoded `ConnectionStrings:DefaultConnection`, `Jwt:Secret`, `RabbitMq:Uri`, and `SeedAdmin:Password` values from `appsettings.json`. These become required environment variables (or `dotnet user-secrets` for local dev); the app fails fast at startup if a required secret is missing in a non-Development environment.
- Document the local setup flow using `dotnet user-secrets` (or a local-only env var file) so a developer can run the API without editing tracked files.
- Document/verify the production flow: environment variables supplied via the container runtime (Docker `-e` / `--env-file`, compose, or the hosting platform's secret store), using the existing `ASPNETCORE_ENVIRONMENT=Production` switch and .NET's colon-to-double-underscore env var mapping (e.g. `ConnectionStrings__DefaultConnection`).
- Add a startup validation step that checks required secret-backed settings are present and fails with a clear error instead of running with empty/default values.

## Capabilities

### New Capabilities
- `environment-config`: How the API loads and validates configuration differently per environment (Development vs Production), and how secrets are supplied outside of tracked files.

### Modified Capabilities
(none — no other capability's requirements change; this introduces the configuration capability that other areas already implicitly depend on)

## Impact

- Affected files: `src/Frozen.API/appsettings.json`, `src/Frozen.API/appsettings.Development.json`, new `src/Frozen.API/appsettings.Production.json`, `src/Frozen.API/Program.cs` (startup validation), `src/Frozen.Infrastructure/DependencyInjection.cs` (wherever `ConnectionStrings`, `Jwt`, `RabbitMq`, `SeedAdmin` are read).
- No new NuGet packages (uses built-in `IConfiguration` + environment variables + `dotnet user-secrets`).
- Deployment impact: production deployments (Dockerfile / docker-compose / hosting platform) must supply the secret env vars going forward; anyone running the container without them will now see a fast, explicit startup failure instead of the app silently using committed defaults.
- Git history impact: the currently committed secret values remain in git history; rotating them (new DB password, new JWT secret, new RabbitMq credentials) is a follow-up operational task, not part of this change's code diff.
