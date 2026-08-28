-- Remove the legacy Customer role and Customer-only accounts (idempotent, transactional).
-- Run via psql against the "admin" schema created by the 20260814101037_InitialCreate
-- migration, AFTER deploying the backend change that stops creating the role:
--   psql -f remove-customer-role.sql
-- Re-running is a no-op (every DELETE removes zero rows once the role is gone).

\set ON_ERROR_STOP on

BEGIN;

-- 1. Remove every UserRoles link to the Customer role.
DELETE FROM admin."UserRoles" ur
USING admin."Roles" r
WHERE r."Id" = ur."RoleId"
  AND r."Name" = 'Customer';

-- 2. Delete accounts left with no role links (legacy Customer-only accounts).
--    Every supported creation path (registration, POST /api/users, the
--    superadmin seed) assigns a role, so a user left with zero roles is a
--    legacy Customer-only account or a broken record that cannot use the panel.
--    Accounts holding any other role keep that role (and the account).
DELETE FROM admin."Users" u
WHERE u."Id" NOT IN (SELECT "UserId" FROM admin."UserRoles");

-- 3. Delete the Customer role row itself.
DELETE FROM admin."Roles" r
WHERE r."Name" = 'Customer';

COMMIT;