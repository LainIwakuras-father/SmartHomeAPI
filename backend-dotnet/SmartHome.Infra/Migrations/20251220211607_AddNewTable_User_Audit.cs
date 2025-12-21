using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace SmartHome.Infra.Migrations
{
    /// <inheritdoc />
    public partial class AddNewTable_User_Audit : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Users",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Username = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Email = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    HashPassword = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Role = table.Column<int>(type: "integer", nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UpdatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Users", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "SecurityAudits",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Timestamp = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "CURRENT_TIMESTAMP"),
                    UserId = table.Column<int>(type: "integer", nullable: true),
                    UserName = table.Column<string>(type: "text", nullable: true),
                    Action = table.Column<string>(type: "text", nullable: false),
                    Resource = table.Column<string>(type: "text", nullable: true),
                    IpAddress = table.Column<string>(type: "character varying(45)", maxLength: 45, nullable: true),
                    IsSuccessful = table.Column<bool>(type: "boolean", nullable: false),
                    Details = table.Column<string>(type: "text", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_SecurityAudits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_SecurityAudits_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "ix_securityaudits_action",
                table: "SecurityAudits",
                column: "Action");

            migrationBuilder.CreateIndex(
                name: "ix_securityaudits_issuccessful",
                table: "SecurityAudits",
                column: "IsSuccessful");

            migrationBuilder.CreateIndex(
                name: "ix_securityaudits_timestamp",
                table: "SecurityAudits",
                column: "Timestamp");

            migrationBuilder.CreateIndex(
                name: "ix_securityaudits_userid",
                table: "SecurityAudits",
                column: "UserId");

            migrationBuilder.CreateIndex(
                name: "ix_users_createdat",
                table: "Users",
                column: "CreatedAt");

            migrationBuilder.CreateIndex(
                name: "ix_users_email",
                table: "Users",
                column: "Email",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "ix_users_role",
                table: "Users",
                column: "Role");

            migrationBuilder.CreateIndex(
                name: "ix_users_username",
                table: "Users",
                column: "Username");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "SecurityAudits");

            migrationBuilder.DropTable(
                name: "Users");
        }
    }
}
