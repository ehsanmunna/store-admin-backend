# Frozen.API

## Configuration

Configuration is layered per ASP.NET Core convention: `appsettings.json` (defaults, no secrets) → `appsettings.{Environment}.json` → environment variables. `ASPNETCORE_ENVIRONMENT` selects the environment (`Development` locally, `Production` when unset/deployed).

The following settings are secrets and are **never** committed to any `appsettings*.json` file:

- `ConnectionStrings:DefaultConnection`
- `Jwt:Secret`
- `RabbitMq:Uri`

### Local development

Use [.NET user-secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) so secrets stay out of the repo entirely:

```bash
cd src/Frozen.API
dotnet user-secrets init
dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Host=localhost;Port=5432;Database=mystore;Username=postgres;Password=your-local-password"
dotnet user-secrets set "Jwt:Secret" "a-long-random-local-dev-secret-at-least-32-chars"
dotnet user-secrets set "RabbitMq:Uri" "amqp://guest:guest@localhost:5672"
```

User-secrets only apply when `ASPNETCORE_ENVIRONMENT=Development` (the default when running via `dotnet run`/Visual Studio without an explicit override), and are stored outside the repo under your user profile.

**Alternative — plain environment variables.** If you'd rather not use user-secrets, set the same values as environment variables before running the app, using `__` in place of `:`:

```bash
export ConnectionStrings__DefaultConnection="Host=localhost;Port=5432;Database=mystore;Username=postgres;Password=your-local-password"
export Jwt__Secret="a-long-random-local-dev-secret-at-least-32-chars"
export RabbitMq__Uri="amqp://guest:guest@localhost:5672"
```

### Production / deployment

Set `ASPNETCORE_ENVIRONMENT=Production` (or leave it unset — Production is the ASP.NET Core default) and supply the secrets as environment variables using the same `__` mapping:

```bash
docker run \
  -e ASPNETCORE_ENVIRONMENT=Production \
  -e ConnectionStrings__DefaultConnection="Host=...;Port=5432;Database=...;Username=...;Password=..." \
  -e Jwt__Secret="a-long-random-production-secret" \
  -e RabbitMq__Uri="amqp://user:pass@rabbitmq-host:5672" \
  -e Cors__AllowedOrigins__0="https://your-production-frontend.example.com" \
  -p 8080:8080 \
  frozen-api
```

With docker-compose, use `environment:` or an `env_file:` (gitignored) instead of inlining values into `docker-compose.yml`.

#### Render.com

If deploying as a Render Web Service built from [src/Frozen.API/Dockerfile](src/Frozen.API/Dockerfile), set the same keys under the service's **Environment** tab (mark the secret-valued ones as **Secret** rather than plain env vars):

- `ASPNETCORE_ENVIRONMENT=Production` — Render does **not** set this by default the way local `dotnet run` defaults to Development, so it must be added explicitly or the app won't load `appsettings.Production.json` / run the fail-fast config check as intended.
- `ConnectionStrings__DefaultConnection`
- `Jwt__Secret`
- `RabbitMq__Uri`
- `Cors__AllowedOrigins__0` (see note below)

No extra port configuration is needed — the Dockerfile already sets `EXPOSE 8080` and `ASPNETCORE_URLS=http://+:8080`, which Render picks up automatically.

These secret env vars are required at **runtime**, not build time — the Dockerfiles publish the app without them, and the container reads them on startup.

If `ConnectionStrings:DefaultConnection`, `Jwt:Secret`, or `RabbitMq:Uri` is missing in any non-Development environment, the API fails to start immediately and logs which setting is missing, instead of starting with an empty value.

`Cors:AllowedOrigins` also needs a real production value (there is no safe default) — set it via `Cors__AllowedOrigins__0`, `Cors__AllowedOrigins__1`, etc., or add it to `appsettings.Production.json`.

### Bootstrap the superadmin

The app does not seed the superadmin automatically. On a fresh database, run the seed once (after migrations are applied) to provision `superadmin@domain.com`, which is granted both the `SuperAdmin` and `Admin` roles — the `Admin` role lets it use every existing admin-gated endpoint.

**Development (defaults):**

```bash
cd backend
./db/seed/seed-superadmin.ps1
```

Uses the development defaults `superadmin@domain.com` / `Superadmin@123` (also documented in `src/Frozen.API/appsettings.Development.json` under `SuperAdmin`), connecting via `ConnectionStrings__DefaultConnection` or the `PGHOST`/`PGPORT`/`PGDATABASE`/`PGUSER`/`PGPASSWORD` env vars.

**Production:**

The seed refuses to run with the development default password when `ASPNETCORE_ENVIRONMENT` is not `Development`. Set these environment variables, then run the same script:

- `SUPERADMIN_EMAIL` — the bootstrap email (defaults to `superadmin@domain.com`)
- `SUPERADMIN_PASSWORD` — a strong password (min 8 characters)
- `ConnectionStrings__DefaultConnection` or the `PG*` vars — database connection

The script generates the ASP.NET Identity password hash via `tools/GeneratePasswordHash` (no hash is ever committed) and runs `db/seed/superadmin.sql` through `psql`. It is idempotent — re-running is a no-op, and an existing account with the same email is left untouched. A missing or too-short `SUPERADMIN_PASSWORD` aborts the run before any write.

### Legacy Customer role cleanup

The `Customer` role is no longer part of the account model — registration only creates `Admin`. Databases created before this change may still hold a `Customer` role row and legacy `Customer`-only accounts (they cannot use the admin panel). Run the cleanup once per database, **after** deploying the backend change (so registration no longer recreates the role):

```bash
cd backend
psql -h localhost -U postgres -d mystore -f db/seed/remove-customer-role.sql
```

The script deletes `Customer` role links, deletes accounts left with no other role (former `Customer`-only accounts), and deletes the `Customer` role row. Accounts holding any other role are kept. It is idempotent — re-running deletes nothing.
