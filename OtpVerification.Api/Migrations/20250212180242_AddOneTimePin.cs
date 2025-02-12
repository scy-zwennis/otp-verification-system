using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace OtpVerification.Api.Migrations
{
    /// <inheritdoc />
    public partial class AddOneTimePin : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "LastIssuedOtpId",
                table: "Users",
                type: "integer",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "OneTimePins",
                columns: table => new
                {
                    OneTimePinId = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UserId = table.Column<int>(type: "integer", nullable: false),
                    Code = table.Column<string>(type: "character varying(6)", maxLength: 6, nullable: false),
                    CreatedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    ExpiresAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    RequestCount = table.Column<int>(type: "integer", nullable: false, defaultValue: 0),
                    IsUsed = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false),
                    UsedAt = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_OneTimePins", x => x.OneTimePinId);
                    table.ForeignKey(
                        name: "FK_OneTimePins_Users_UserId",
                        column: x => x.UserId,
                        principalTable: "Users",
                        principalColumn: "UserId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Users_LastIssuedOtpId",
                table: "Users",
                column: "LastIssuedOtpId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_OneTimePins_UserId_CreatedAt",
                table: "OneTimePins",
                columns: new[] { "UserId", "CreatedAt" });

            migrationBuilder.AddForeignKey(
                name: "FK_Users_OneTimePins_LastIssuedOtpId",
                table: "Users",
                column: "LastIssuedOtpId",
                principalTable: "OneTimePins",
                principalColumn: "OneTimePinId",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Users_OneTimePins_LastIssuedOtpId",
                table: "Users");

            migrationBuilder.DropTable(
                name: "OneTimePins");

            migrationBuilder.DropIndex(
                name: "IX_Users_LastIssuedOtpId",
                table: "Users");

            migrationBuilder.DropColumn(
                name: "LastIssuedOtpId",
                table: "Users");
        }
    }
}
