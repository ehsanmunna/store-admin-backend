-- Seed the superadmin bootstrap credential (idempotent, transactional).
-- Run via psql, passing the two required variables:
--   psql -v superadmin_email=... -v superadmin_password_hash=... -f superadmin.sql
-- (or use the seed-superadmin.ps1 wrapper). Targets the "admin" schema
-- created by the 20260814101037_InitialCreate migration.

\set ON_ERROR_STOP on

-- Fail loudly when a required variable was not passed to psql at all.
\if :{?superadmin_email}
\else
\echo 'ERROR: superadmin_email variable is not defined. Set it with -v superadmin_email=...'
SELECT 1 / 0;
\endif
\if :{?superadmin_password_hash}
\else
\echo 'ERROR: superadmin_password_hash variable is not defined. Set it with -v superadmin_password_hash=...'
SELECT 1 / 0;
\endif

-- Fail loudly when a required variable is defined but empty. psql substitutes
-- :'var' here (outside dollar-quotes), so an empty value makes the condition
-- true and the expression raises a division-by-zero error, aborting the run.
SELECT CASE WHEN :'superadmin_email' = '' OR :'superadmin_password_hash' = ''
            THEN 1 / 0
            ELSE 1
       END;

BEGIN;

-- Ensure the roles exist. SuperAdmin is created here; Admin is normally
-- created lazily by the app, but on a truly fresh database it may not exist
-- yet, so ensure it too (the UserRoles FK requires it).
INSERT INTO admin."Roles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
SELECT gen_random_uuid(), 'SuperAdmin', 'SUPERADMIN', gen_random_uuid()
WHERE NOT EXISTS (SELECT 1 FROM admin."Roles" WHERE "Name" = 'SuperAdmin');

INSERT INTO admin."Roles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
SELECT gen_random_uuid(), 'Admin', 'ADMIN', gen_random_uuid()
WHERE NOT EXISTS (SELECT 1 FROM admin."Roles" WHERE "Name" = 'Admin');

-- Create the superadmin user only if no user already holds that email
-- (open registration may have created one already -- leave it untouched).
INSERT INTO admin."Users"
    ("Id", "FirstName", "LastName", "CreatedAt", "UserName", "NormalizedUserName",
     "Email", "NormalizedEmail", "EmailConfirmed", "PasswordHash",
     "SecurityStamp", "ConcurrencyStamp", "PhoneNumber", "PhoneNumberConfirmed",
     "TwoFactorEnabled", "LockoutEnd", "LockoutEnabled", "AccessFailedCount")
SELECT gen_random_uuid(), 'Super', 'Admin', now(),
       :'superadmin_email', upper(:'superadmin_email'),
       :'superadmin_email', upper(:'superadmin_email'),
       true, :'superadmin_password_hash',
       gen_random_uuid(), gen_random_uuid(), NULL, false,
       false, NULL, true, 0
WHERE NOT EXISTS (
    SELECT 1 FROM admin."Users" WHERE "NormalizedEmail" = upper(:'superadmin_email')
);

-- Link the user to the SuperAdmin and Admin roles (no-op on re-runs).
INSERT INTO admin."UserRoles" ("UserId", "RoleId")
SELECT u."Id", r."Id"
FROM admin."Users" u
JOIN admin."Roles" r ON r."Name" IN ('SuperAdmin', 'Admin')
WHERE u."NormalizedEmail" = upper(:'superadmin_email')
ON CONFLICT ("UserId", "RoleId") DO NOTHING;

COMMIT;