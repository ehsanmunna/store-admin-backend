using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Frozen.Infrastructure.Persistence.Migrations
{
    /// <inheritdoc />
    public partial class UnifyStoreSchema : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            // EF Core's RenameTable no-ops when Name == NewName, even across a schema change
            // (an Npgsql provider quirk) - move each table with an explicit SET SCHEMA instead.
            migrationBuilder.Sql("ALTER TABLE admin.\"UserTokens\" SET SCHEMA public;");
            migrationBuilder.Sql("ALTER TABLE admin.\"Users\" SET SCHEMA public;");
            migrationBuilder.Sql("ALTER TABLE admin.\"UserRoles\" SET SCHEMA public;");
            migrationBuilder.Sql("ALTER TABLE admin.\"UserLogins\" SET SCHEMA public;");
            migrationBuilder.Sql("ALTER TABLE admin.\"UserClaims\" SET SCHEMA public;");
            migrationBuilder.Sql("ALTER TABLE admin.\"Suppliers\" SET SCHEMA public;");
            migrationBuilder.Sql("ALTER TABLE admin.\"Roles\" SET SCHEMA public;");
            migrationBuilder.Sql("ALTER TABLE admin.\"RoleClaims\" SET SCHEMA public;");
            migrationBuilder.Sql("ALTER TABLE admin.\"Products\" SET SCHEMA public;");
            migrationBuilder.Sql("ALTER TABLE admin.\"Orders\" SET SCHEMA public;");
            migrationBuilder.Sql("ALTER TABLE admin.\"OrderItems\" SET SCHEMA public;");
            migrationBuilder.Sql("ALTER TABLE admin.\"Categories\" SET SCHEMA public;");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Orders",
                type: "character varying(50)",
                maxLength: 50,
                nullable: false,
                oldClrType: typeof(int),
                oldType: "integer");

            migrationBuilder.AlterColumn<string>(
                name: "ShippingState",
                table: "Orders",
                type: "character varying(150)",
                maxLength: 150,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCountry",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: true,
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.EnsureSchema(
                name: "admin");

            migrationBuilder.Sql("ALTER TABLE public.\"UserTokens\" SET SCHEMA admin;");
            migrationBuilder.Sql("ALTER TABLE public.\"Users\" SET SCHEMA admin;");
            migrationBuilder.Sql("ALTER TABLE public.\"UserRoles\" SET SCHEMA admin;");
            migrationBuilder.Sql("ALTER TABLE public.\"UserLogins\" SET SCHEMA admin;");
            migrationBuilder.Sql("ALTER TABLE public.\"UserClaims\" SET SCHEMA admin;");
            migrationBuilder.Sql("ALTER TABLE public.\"Suppliers\" SET SCHEMA admin;");
            migrationBuilder.Sql("ALTER TABLE public.\"Roles\" SET SCHEMA admin;");
            migrationBuilder.Sql("ALTER TABLE public.\"RoleClaims\" SET SCHEMA admin;");
            migrationBuilder.Sql("ALTER TABLE public.\"Products\" SET SCHEMA admin;");
            migrationBuilder.Sql("ALTER TABLE public.\"Orders\" SET SCHEMA admin;");
            migrationBuilder.Sql("ALTER TABLE public.\"OrderItems\" SET SCHEMA admin;");
            migrationBuilder.Sql("ALTER TABLE public.\"Categories\" SET SCHEMA admin;");

            migrationBuilder.AlterColumn<int>(
                name: "Status",
                schema: "admin",
                table: "Orders",
                type: "integer",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "character varying(50)",
                oldMaxLength: 50);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingState",
                schema: "admin",
                table: "Orders",
                type: "character varying(150)",
                maxLength: 150,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(150)",
                oldMaxLength: 150,
                oldNullable: true);

            migrationBuilder.AlterColumn<string>(
                name: "ShippingCountry",
                schema: "admin",
                table: "Orders",
                type: "character varying(100)",
                maxLength: 100,
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "character varying(100)",
                oldMaxLength: 100,
                oldNullable: true);
        }
    }
}
