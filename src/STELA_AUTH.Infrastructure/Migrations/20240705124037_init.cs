using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace STELA_AUTH.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class init : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Accounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    Phone = table.Column<string>(type: "text", nullable: true),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    RestoreCode = table.Column<string>(type: "text", nullable: true),
                    RoleName = table.Column<string>(type: "text", nullable: false),
                    RestoreCodeValidBefore = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    WasPasswordResetRequest = table.Column<bool>(type: "boolean", nullable: false),
                    Token = table.Column<string>(type: "text", nullable: true),
                    TokenValidBefore = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    Image = table.Column<string>(type: "text", nullable: true),
                    ConfirmationCode = table.Column<string>(type: "text", nullable: true),
                    ConfirmationCodeValidBefore = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    IsEmailVerified = table.Column<bool>(type: "boolean", nullable: false),
                    LastPasswordDateModified = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Accounts", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "UnconfirmedAccounts",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uuid", nullable: false),
                    Email = table.Column<string>(type: "character varying(256)", maxLength: 256, nullable: false),
                    FirstName = table.Column<string>(type: "text", nullable: false),
                    LastName = table.Column<string>(type: "text", nullable: false),
                    PasswordHash = table.Column<string>(type: "text", nullable: false),
                    ConfirmationCode = table.Column<string>(type: "text", nullable: true),
                    ConfirmationCodeValidBefore = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_UnconfirmedAccounts", x => x.Id);
                });
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Accounts");

            migrationBuilder.DropTable(
                name: "UnconfirmedAccounts");
        }
    }
}
