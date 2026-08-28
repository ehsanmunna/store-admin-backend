--
-- Production initialization script for the "mystore" database.
--
-- Contents:
--   1. Full schema (all tables, indexes, foreign keys) for the shared `public`
--      schema, as currently defined by store-admin's EF Core migrations
--      (Products/Categories/Orders/OrderItems/Suppliers/Identity tables) and
--      store-shop's TypeORM-owned tables (users, password_reset_tokens).
--   2. NO application data, except:
--        - the built-in "Admin" and "SuperAdmin" roles
--        - one bootstrap super admin account
--        - EF Core's migration-history bookkeeping rows (see note below)
--
-- Generated from the current local dev database schema on 2026-08-29.
-- Review before running against production.
--
-- IMPORTANT - bootstrap password:
--   A random bootstrap password was generated for the super admin account.
--   It is NOT embedded in this file. It was shown once in the chat that
--   generated this script. Log in with it immediately after running this
--   script, then change it (Users > Reset password) or rotate it via the
--   admin panel. Do not reuse the local dev environment's superadmin
--   password for production.
--
-- IMPORTANT - EF Core migrations bookkeeping:
--   The two INSERTs into "__EFMigrationsHistory" record that migrations
--   20260814101037_InitialCreate and 20260828135629_UnifyStoreSchema have
--   already been applied (because their effects are already baked into the
--   CREATE TABLE statements below). Without these rows, a future
--   `dotnet ef database update` against this database would try to re-run
--   those migrations from scratch and fail against tables that already
--   exist. If you add new EF Core migrations after this point, they will
--   apply normally on top of this baseline.
--
-- Usage:
--   psql "<production-connection-string>" -f init-production.sql
--
-- This script is idempotent for CREATE SCHEMA only; it is NOT safe to run
-- twice against a database that already has these tables (CREATE TABLE will
-- fail on the second run). Run it once, against a fresh database.
--

BEGIN;

SET statement_timeout = 0;
SET lock_timeout = 0;
SET idle_in_transaction_session_timeout = 0;
SET client_encoding = 'UTF8';
SET standard_conforming_strings = on;
SELECT pg_catalog.set_config('search_path', '', false);
SET check_function_bodies = false;
SET xmloption = content;
SET client_min_messages = warning;
SET row_security = off;

--
-- Schema (every Postgres database already has a "public" schema by default,
-- so this is safe even though pg_dump normally emits a plain CREATE SCHEMA).
--

CREATE SCHEMA IF NOT EXISTS public;
COMMENT ON SCHEMA public IS 'standard public schema';

SET default_tablespace = '';
SET default_table_access_method = heap;

--
-- Name: Categories; Type: TABLE; Schema: public
--

CREATE TABLE public."Categories" (
    "Id" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "Slug" character varying(200) NOT NULL,
    "Description" character varying(1000),
    "ParentCategoryId" uuid,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);

--
-- Name: OrderItems; Type: TABLE; Schema: public
--

CREATE TABLE public."OrderItems" (
    "Id" uuid NOT NULL,
    "OrderId" uuid NOT NULL,
    "ProductId" uuid NOT NULL,
    "ProductName" character varying(300) NOT NULL,
    "Quantity" integer NOT NULL,
    "UnitPrice" numeric(18,2) NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);

--
-- Name: Orders; Type: TABLE; Schema: public
--

CREATE TABLE public."Orders" (
    "Id" uuid NOT NULL,
    "OrderNumber" character varying(50) NOT NULL,
    "UserId" uuid NOT NULL,
    "Status" character varying(50) NOT NULL,
    "ShippingFullName" character varying(200) NOT NULL,
    "ShippingAddressLine1" character varying(300) NOT NULL,
    "ShippingAddressLine2" character varying(300),
    "ShippingCity" character varying(150) NOT NULL,
    "ShippingState" character varying(150),
    "ShippingPostalCode" character varying(20) NOT NULL,
    "ShippingCountry" character varying(100),
    "ShippingPhone" character varying(30),
    "SubTotal" numeric(18,2) NOT NULL,
    "ShippingCost" numeric(18,2) NOT NULL,
    "Tax" numeric(18,2) NOT NULL,
    "TotalAmount" numeric(18,2) NOT NULL,
    "Notes" character varying(1000),
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);

--
-- Name: Products; Type: TABLE; Schema: public
--

CREATE TABLE public."Products" (
    "Id" uuid NOT NULL,
    "Name" character varying(300) NOT NULL,
    "Slug" character varying(300) NOT NULL,
    "Description" character varying(4000),
    "Sku" character varying(100) NOT NULL,
    "Price" numeric(18,2) NOT NULL,
    "CompareAtPrice" numeric(18,2),
    "CostPrice" numeric(18,2) NOT NULL,
    "StockQuantity" integer NOT NULL,
    "ImageUrl" character varying(1000),
    "IsActive" boolean NOT NULL,
    "IsFeatured" boolean NOT NULL,
    "CategoryId" uuid NOT NULL,
    "SupplierId" uuid NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);

--
-- Name: RoleClaims; Type: TABLE; Schema: public
--

CREATE TABLE public."RoleClaims" (
    "Id" integer NOT NULL,
    "RoleId" uuid NOT NULL,
    "ClaimType" text,
    "ClaimValue" text
);

ALTER TABLE public."RoleClaims" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."RoleClaims_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

--
-- Name: Roles; Type: TABLE; Schema: public
--

CREATE TABLE public."Roles" (
    "Id" uuid NOT NULL,
    "Name" character varying(256),
    "NormalizedName" character varying(256),
    "ConcurrencyStamp" text
);

--
-- Name: Suppliers; Type: TABLE; Schema: public
--

CREATE TABLE public."Suppliers" (
    "Id" uuid NOT NULL,
    "Name" character varying(200) NOT NULL,
    "ContactEmail" character varying(200),
    "Website" character varying(300),
    "Notes" character varying(1000),
    "IsActive" boolean NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UpdatedAt" timestamp with time zone
);

--
-- Name: UserClaims; Type: TABLE; Schema: public
--

CREATE TABLE public."UserClaims" (
    "Id" integer NOT NULL,
    "UserId" uuid NOT NULL,
    "ClaimType" text,
    "ClaimValue" text
);

ALTER TABLE public."UserClaims" ALTER COLUMN "Id" ADD GENERATED BY DEFAULT AS IDENTITY (
    SEQUENCE NAME public."UserClaims_Id_seq"
    START WITH 1
    INCREMENT BY 1
    NO MINVALUE
    NO MAXVALUE
    CACHE 1
);

--
-- Name: UserLogins; Type: TABLE; Schema: public
--

CREATE TABLE public."UserLogins" (
    "LoginProvider" text NOT NULL,
    "ProviderKey" text NOT NULL,
    "ProviderDisplayName" text,
    "UserId" uuid NOT NULL
);

--
-- Name: UserRoles; Type: TABLE; Schema: public
--

CREATE TABLE public."UserRoles" (
    "UserId" uuid NOT NULL,
    "RoleId" uuid NOT NULL
);

--
-- Name: UserTokens; Type: TABLE; Schema: public
--

CREATE TABLE public."UserTokens" (
    "UserId" uuid NOT NULL,
    "LoginProvider" text NOT NULL,
    "Name" text NOT NULL,
    "Value" text
);

--
-- Name: Users; Type: TABLE; Schema: public
--

CREATE TABLE public."Users" (
    "Id" uuid NOT NULL,
    "FirstName" text NOT NULL,
    "LastName" text NOT NULL,
    "CreatedAt" timestamp with time zone NOT NULL,
    "UserName" character varying(256),
    "NormalizedUserName" character varying(256),
    "Email" character varying(256),
    "NormalizedEmail" character varying(256),
    "EmailConfirmed" boolean NOT NULL,
    "PasswordHash" text,
    "SecurityStamp" text,
    "ConcurrencyStamp" text,
    "PhoneNumber" text,
    "PhoneNumberConfirmed" boolean NOT NULL,
    "TwoFactorEnabled" boolean NOT NULL,
    "LockoutEnd" timestamp with time zone,
    "LockoutEnabled" boolean NOT NULL,
    "AccessFailedCount" integer NOT NULL
);

--
-- Name: __EFMigrationsHistory; Type: TABLE; Schema: public
--

CREATE TABLE public."__EFMigrationsHistory" (
    "MigrationId" character varying(150) NOT NULL,
    "ProductVersion" character varying(32) NOT NULL
);

--
-- Name: password_reset_tokens; Type: TABLE; Schema: public (store-shop owned)
--

CREATE TABLE public.password_reset_tokens (
    "Id" uuid NOT NULL,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "UpdatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    user_id character varying NOT NULL,
    token_hash character varying NOT NULL,
    expires_at timestamp without time zone NOT NULL,
    used_at timestamp without time zone
);

--
-- Name: users; Type: TABLE; Schema: public (store-shop owned, storefront customers)
--

CREATE TABLE public.users (
    "Id" uuid NOT NULL,
    "CreatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    "UpdatedAt" timestamp without time zone DEFAULT now() NOT NULL,
    email character varying NOT NULL,
    password_hash character varying NOT NULL,
    first_name character varying NOT NULL,
    last_name character varying NOT NULL
);

--
-- Primary keys
--

ALTER TABLE ONLY public.password_reset_tokens
    ADD CONSTRAINT "PK_53d2f584f1aee705c114bf6d1a9" PRIMARY KEY ("Id");

ALTER TABLE ONLY public.users
    ADD CONSTRAINT "PK_99a6fd82d4e6f4faed5eee5d00a" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."Categories"
    ADD CONSTRAINT "PK_Categories" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."OrderItems"
    ADD CONSTRAINT "PK_OrderItems" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."Orders"
    ADD CONSTRAINT "PK_Orders" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."Products"
    ADD CONSTRAINT "PK_Products" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."RoleClaims"
    ADD CONSTRAINT "PK_RoleClaims" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."Roles"
    ADD CONSTRAINT "PK_Roles" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."Suppliers"
    ADD CONSTRAINT "PK_Suppliers" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."UserClaims"
    ADD CONSTRAINT "PK_UserClaims" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."UserLogins"
    ADD CONSTRAINT "PK_UserLogins" PRIMARY KEY ("LoginProvider", "ProviderKey");

ALTER TABLE ONLY public."UserRoles"
    ADD CONSTRAINT "PK_UserRoles" PRIMARY KEY ("UserId", "RoleId");

ALTER TABLE ONLY public."UserTokens"
    ADD CONSTRAINT "PK_UserTokens" PRIMARY KEY ("UserId", "LoginProvider", "Name");

ALTER TABLE ONLY public."Users"
    ADD CONSTRAINT "PK_Users" PRIMARY KEY ("Id");

ALTER TABLE ONLY public."__EFMigrationsHistory"
    ADD CONSTRAINT "PK___EFMigrationsHistory" PRIMARY KEY ("MigrationId");

--
-- Indexes
--

CREATE INDEX "EmailIndex" ON public."Users" USING btree ("NormalizedEmail");
CREATE INDEX "IDX_52ac39dd8a28730c63aeb428c9" ON public.password_reset_tokens USING btree (user_id);
CREATE UNIQUE INDEX "IDX_97672ac88f789774dd47f7c8be" ON public.users USING btree (email);
CREATE INDEX "IX_Categories_ParentCategoryId" ON public."Categories" USING btree ("ParentCategoryId");
CREATE UNIQUE INDEX "IX_Categories_Slug" ON public."Categories" USING btree ("Slug");
CREATE INDEX "IX_OrderItems_OrderId" ON public."OrderItems" USING btree ("OrderId");
CREATE INDEX "IX_OrderItems_ProductId" ON public."OrderItems" USING btree ("ProductId");
CREATE UNIQUE INDEX "IX_Orders_OrderNumber" ON public."Orders" USING btree ("OrderNumber");
CREATE INDEX "IX_Products_CategoryId" ON public."Products" USING btree ("CategoryId");
CREATE UNIQUE INDEX "IX_Products_Sku" ON public."Products" USING btree ("Sku");
CREATE UNIQUE INDEX "IX_Products_Slug" ON public."Products" USING btree ("Slug");
CREATE INDEX "IX_Products_SupplierId" ON public."Products" USING btree ("SupplierId");
CREATE INDEX "IX_RoleClaims_RoleId" ON public."RoleClaims" USING btree ("RoleId");
CREATE INDEX "IX_UserClaims_UserId" ON public."UserClaims" USING btree ("UserId");
CREATE INDEX "IX_UserLogins_UserId" ON public."UserLogins" USING btree ("UserId");
CREATE INDEX "IX_UserRoles_RoleId" ON public."UserRoles" USING btree ("RoleId");
CREATE UNIQUE INDEX "RoleNameIndex" ON public."Roles" USING btree ("NormalizedName");
CREATE UNIQUE INDEX "UserNameIndex" ON public."Users" USING btree ("NormalizedUserName");

--
-- Foreign keys
--

ALTER TABLE ONLY public."Categories"
    ADD CONSTRAINT "FK_Categories_Categories_ParentCategoryId" FOREIGN KEY ("ParentCategoryId") REFERENCES public."Categories"("Id") ON DELETE RESTRICT;

ALTER TABLE ONLY public."OrderItems"
    ADD CONSTRAINT "FK_OrderItems_Orders_OrderId" FOREIGN KEY ("OrderId") REFERENCES public."Orders"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."OrderItems"
    ADD CONSTRAINT "FK_OrderItems_Products_ProductId" FOREIGN KEY ("ProductId") REFERENCES public."Products"("Id") ON DELETE RESTRICT;

ALTER TABLE ONLY public."Products"
    ADD CONSTRAINT "FK_Products_Categories_CategoryId" FOREIGN KEY ("CategoryId") REFERENCES public."Categories"("Id") ON DELETE RESTRICT;

ALTER TABLE ONLY public."Products"
    ADD CONSTRAINT "FK_Products_Suppliers_SupplierId" FOREIGN KEY ("SupplierId") REFERENCES public."Suppliers"("Id") ON DELETE RESTRICT;

ALTER TABLE ONLY public."RoleClaims"
    ADD CONSTRAINT "FK_RoleClaims_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."Roles"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."UserClaims"
    ADD CONSTRAINT "FK_UserClaims_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."UserLogins"
    ADD CONSTRAINT "FK_UserLogins_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."UserRoles"
    ADD CONSTRAINT "FK_UserRoles_Roles_RoleId" FOREIGN KEY ("RoleId") REFERENCES public."Roles"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."UserRoles"
    ADD CONSTRAINT "FK_UserRoles_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE;

ALTER TABLE ONLY public."UserTokens"
    ADD CONSTRAINT "FK_UserTokens_Users_UserId" FOREIGN KEY ("UserId") REFERENCES public."Users"("Id") ON DELETE CASCADE;

--
-- ============================================================
-- Seed data: built-in roles + one bootstrap super admin account
-- ============================================================
--

INSERT INTO public."Roles" ("Id", "Name", "NormalizedName", "ConcurrencyStamp")
VALUES
    (gen_random_uuid(), 'Admin', 'ADMIN', gen_random_uuid()::text),
    (gen_random_uuid(), 'SuperAdmin', 'SUPERADMIN', gen_random_uuid()::text);

-- Bootstrap super admin account.
-- Email below is a placeholder - change it before running.
-- PasswordHash below was generated for a random bootstrap password shown
-- once in chat (not stored in this file) - log in and rotate it immediately.
INSERT INTO public."Users" (
    "Id", "FirstName", "LastName", "CreatedAt",
    "UserName", "NormalizedUserName", "Email", "NormalizedEmail",
    "EmailConfirmed", "PasswordHash", "SecurityStamp", "ConcurrencyStamp",
    "PhoneNumberConfirmed", "TwoFactorEnabled", "LockoutEnabled", "AccessFailedCount"
)
VALUES (
    gen_random_uuid(), 'Super', 'Admin', now(),
    'superadmin@domain.com', 'SUPERADMIN@DOMAIN.COM', 'superadmin@domain.com', 'SUPERADMIN@DOMAIN.COM',
    true,
    'AQAAAAIAAYagAAAAEPaFXyW0w2mKJn7sUqT32TQ6xYdd/ndsQYvNFViIGckoz6QXDUSewb+TkuxUEuBgxQ==',
    gen_random_uuid()::text, gen_random_uuid()::text,
    false, false, true, 0
);

-- Grant both built-in roles to the bootstrap account.
INSERT INTO public."UserRoles" ("UserId", "RoleId")
SELECT u."Id", r."Id"
FROM public."Users" u
CROSS JOIN public."Roles" r
WHERE u."Email" = 'superadmin@domain.com'
  AND r."Name" IN ('Admin', 'SuperAdmin');

--
-- EF Core migration-history bookkeeping (see note at top of file).
--

INSERT INTO public."__EFMigrationsHistory" ("MigrationId", "ProductVersion")
VALUES
    ('20260814101037_InitialCreate', '8.0.10'),
    ('20260828135629_UnifyStoreSchema', '8.0.10');

COMMIT;
