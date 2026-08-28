<#
.SYNOPSIS
    Seeds the superadmin bootstrap credential into the database (idempotent).

.DESCRIPTION
    Runs db/seed/superadmin.sql via psql, generating the ASP.NET Identity
    password hash with the GeneratePasswordHash helper. Reads credentials from
    environment variables with documented development defaults.

    Environment variables:
      SUPERADMIN_EMAIL     Email for the bootstrap account (default: superadmin@domain.com)
      SUPERADMIN_PASSWORD  Password for the bootstrap account (default: Superadmin@123, dev only)

    Database connection:
      ConnectionStrings__DefaultConnection (preferred), e.g.
        "Host=localhost;Port=5432;Database=mystore;Username=postgres;Password=..."
      ...or the individual PGHOST / PGPORT / PGDATABASE / PGUSER / PGPASSWORD vars.

.PARAMETER Email
    Overrides SUPERADMIN_EMAIL.

.PARAMETER Password
    Overrides SUPERADMIN_PASSWORD.

.PARAMETER Force
    Allow the dev default password even when ASPNETCORE_ENVIRONMENT is not Development.
#>
[CmdletBinding()]
param(
    [string]$Email,
    [string]$Password,
    [switch]$Force
)

$ErrorActionPreference = 'Stop'

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$repoRoot  = Split-Path -Parent (Split-Path -Parent $scriptDir)

$DevDefaultPassword = 'Superadmin@123'

# --- Credentials -----------------------------------------------------------

$email = if ($Email) { $Email } else { $env:SUPERADMIN_EMAIL }
if ([string]::IsNullOrWhiteSpace($email)) { $email = 'superadmin@domain.com' }

$password = if ($Password) { $Password } else { $env:SUPERADMIN_PASSWORD }
$usingDefault = $false
if ([string]::IsNullOrWhiteSpace($password)) {
    $password = $DevDefaultPassword
    $usingDefault = $true
}

if ($password.Length -lt 8) {
    throw "SUPERADMIN_PASSWORD must be at least 8 characters (got $($password.Length))."
}

$environment = $env:ASPNETCORE_ENVIRONMENT
if ([string]::IsNullOrWhiteSpace($environment)) { $environment = 'Development' }

if ($usingDefault -and $environment -ne 'Development' -and -not $Force) {
    throw "Refusing to use the development default password in environment '$environment'. " +
          "Set SUPERADMIN_PASSWORD explicitly (or pass -Force to override)."
}

# --- Password hash ---------------------------------------------------------

$helperProject = Join-Path $repoRoot 'tools\GeneratePasswordHash\GeneratePasswordHash.csproj'
$helperDll     = Join-Path $repoRoot 'tools\GeneratePasswordHash\bin\Release\net8.0\GeneratePasswordHash.dll'

if (-not (Test-Path $helperDll)) {
    Write-Host 'Building GeneratePasswordHash...'
    & dotnet build $helperProject -c Release -v q --nologo
    if ($LASTEXITCODE -ne 0) { throw 'Failed to build GeneratePasswordHash.' }
}

$hash = (& dotnet $helperDll $password).Trim()
if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($hash)) {
    throw 'Failed to generate the password hash.'
}

# --- Database connection ----------------------------------------------------

$conn = $env:ConnectionStrings__DefaultConnection
if ($conn) {
    $map = @{}
    foreach ($part in ($conn -split ';')) {
        if ($part -match '^\s*([^=]+)=(.*)$') {
            $map[$matches[1].Trim()] = $matches[2].Trim()
        }
    }
    if ($map['Host'])     { $env:PGHOST     = $map['Host'] }
    if ($map['Port'])     { $env:PGPORT     = $map['Port'] }
    if ($map['Database']) { $env:PGDATABASE = $map['Database'] }
    if ($map['Username']) { $env:PGUSER     = $map['Username'] }
    if ($map['Password']) { $env:PGPASSWORD = $map['Password'] }
}

$required = @('PGHOST', 'PGPORT', 'PGDATABASE', 'PGUSER', 'PGPASSWORD')
$missing = @($required | Where-Object {
    $v = (Get-Item "env:$($_)" -ErrorAction SilentlyContinue).Value
    [string]::IsNullOrWhiteSpace($v)
})
if ($missing.Count -gt 0) {
    throw "Missing database connection settings: $($missing -join ', '). " +
          'Set ConnectionStrings__DefaultConnection or the PGHOST/PGPORT/PGDATABASE/PGUSER/PGPASSWORD env vars.'
}

# --- Seed -------------------------------------------------------------------

$sqlFile = Join-Path $scriptDir 'superadmin.sql'
Write-Host "Seeding superadmin '$email' into database '$($env:PGDATABASE)'..."
& psql -v "superadmin_email=$email" -v "superadmin_password_hash=$hash" -f $sqlFile
if ($LASTEXITCODE -ne 0) {
    throw "psql exited with code $LASTEXITCODE."
}

Write-Host 'Superadmin seed completed successfully.'