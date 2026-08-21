-- Seed the superadmin bootstrap credential (idempotent, transactional).
--
-- pgAdmin-friendly version: plain SQL only, no psql meta-commands
-- (\set / \if / \echo / :'var' are NOT supported by pgAdmin's Query Tool).
--
-- HOW TO RUN:
--   1. Open pgAdmin -> select the target database (e.g. "mystore") -> Query Tool.
--   2. Paste this whole file and run it (F5 / Execute).
--
-- BEFORE RUNNING: edit the two constants below if you want a different
-- email or password hash. The embedded hash is the ASP.NET Identity hash of
-- the dev default password 'Superadmin@123'. To use another password, generate
-- a new hash with:
--   dotnet run --project backend/tools/GeneratePasswordHash -c Release -- <your-password>
-- and replace the value of v_password_hash.
--
-- Targets the "admin" schema created by the 20260814101037_InitialCreate migration.

BEGIN;

DO $$
DECLARE
    v_email            text := 'superadmin@domain.com';
    v_password_hash    text := 'AQAAAAIAAYagAAAAENn1DlilnqJImGxaN+RuW57rThMSwicgj4m0agPysPTZAVDfdaUR50+WHSTyy1soYA==';
BEGIN
    -- Fail loudly if the constants were accidentally blanked out.
    IF v_email = '' OR v_password_hash = '' THEN
        RAISE EXCEPTION 'superadmin email or password hash is empty - edit the constants at the top of this script';
    END IF;

    -- Ensure the roles exist. SuperAdmin is created here; Admin is normally
    -- created lazily by the app, but on a truly fresh database it may not
    -- exist yet, so ensure it too (the UserRoles FK requires it).
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
           v_email, upper(v_email),
           v_email, upper(v_email),
           true, v_password_hash,
           gen_random_uuid(), gen_random_uuid(), NULL, false,
           false, NULL, true, 0
    WHERE NOT EXISTS (
        SELECT 1 FROM admin."Users" WHERE "NormalizedEmail" = upper(v_email)
    );

    -- Link the user to the SuperAdmin and Admin roles (no-op on re-runs).
    INSERT INTO admin."UserRoles" ("UserId", "RoleId")
    SELECT u."Id", r."Id"
    FROM admin."Users" u
    JOIN admin."Roles" r ON r."Name" IN ('SuperAdmin', 'Admin')
    WHERE u."NormalizedEmail" = upper(v_email)
    ON CONFLICT ("UserId", "RoleId") DO NOTHING;
END $$;

COMMIT;
