## Purpose

Defines how the API determines which environment it is running in, which configuration values come from tracked files versus environment-supplied secrets, and how it behaves when required secrets are missing.

## ADDED Requirements

### Requirement: Environment-specific configuration files
The system SHALL select configuration overrides based on the ASP.NET Core environment name (`ASPNETCORE_ENVIRONMENT`), loading `appsettings.{Environment}.json` on top of `appsettings.json` for at least the `Development` and `Production` environments.

#### Scenario: Running locally in Development
- **WHEN** the API starts with `ASPNETCORE_ENVIRONMENT=Development`
- **THEN** settings from `appsettings.Development.json` override the base `appsettings.json` values

#### Scenario: Running in Production
- **WHEN** the API starts with `ASPNETCORE_ENVIRONMENT=Production`
- **THEN** settings from `appsettings.Production.json` override the base `appsettings.json` values

### Requirement: No secrets in tracked configuration files
Tracked configuration files (`appsettings.json`, `appsettings.Development.json`, `appsettings.Production.json`) SHALL NOT contain real secret values for the database connection string, JWT signing key, RabbitMQ connection URI, or admin seed password. These values SHALL be supplied only through environment variables or a local developer secret store outside of source control.

#### Scenario: Inspecting tracked files for secrets
- **WHEN** any tracked `appsettings*.json` file is inspected
- **THEN** the `ConnectionStrings:DefaultConnection`, `Jwt:Secret`, `RabbitMq:Uri`, and `SeedAdmin:Password` keys are absent or contain only non-functional placeholders, never a working credential

### Requirement: Environment variables override file-based configuration
The system SHALL read configuration from environment variables, using the standard ASP.NET Core mapping where `:` in a configuration key is expressed as `__` in the environment variable name (e.g. `ConnectionStrings__DefaultConnection`), and environment variables SHALL take precedence over any tracked JSON file.

#### Scenario: Supplying the database connection string in production
- **WHEN** the environment variable `ConnectionStrings__DefaultConnection` is set to a production connection string
- **THEN** the API connects to that database instead of any value present in `appsettings.json`

#### Scenario: Supplying the JWT secret in production
- **WHEN** the environment variable `Jwt__Secret` is set
- **THEN** the API signs and validates tokens using that secret instead of any value present in `appsettings.json`

### Requirement: Fail fast when required secrets are missing outside Development
The system SHALL validate at startup that the database connection string, JWT secret, and RabbitMQ connection URI are present and non-empty whenever the environment is not `Development`. If any is missing, the system SHALL stop startup with an error identifying which setting is missing, rather than starting with an empty or default value.

#### Scenario: Missing JWT secret in Production
- **WHEN** the API starts with `ASPNETCORE_ENVIRONMENT=Production` and no `Jwt__Secret` environment variable or configuration value is set
- **THEN** the API fails to start and logs an error naming the missing `Jwt:Secret` setting

#### Scenario: Missing database connection string in Production
- **WHEN** the API starts with `ASPNETCORE_ENVIRONMENT=Production` and no database connection string is configured
- **THEN** the API fails to start and logs an error naming the missing `ConnectionStrings:DefaultConnection` setting

#### Scenario: Local development without production secrets
- **WHEN** the API starts with `ASPNETCORE_ENVIRONMENT=Development` and uses locally-configured development values (via `dotnet user-secrets` or local environment variables)
- **THEN** the API starts normally without requiring the same validation to reject placeholder-free local values, as long as the Development values themselves are non-empty

### Requirement: Documented local developer setup
The system SHALL provide documentation describing how a developer supplies local secret values (database connection string, JWT secret, RabbitMQ URI, admin seed password) via `dotnet user-secrets` or local environment variables without editing any tracked `appsettings*.json` file.

#### Scenario: New developer onboarding
- **WHEN** a developer clones the repository and follows the documented setup steps
- **THEN** they can run the API locally against their own database and message broker without modifying or committing any secret value
