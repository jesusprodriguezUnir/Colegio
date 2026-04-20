using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Colegio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddSchoolDetails : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "CIF",
                table: "Schools",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Schools",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "PostalCode",
                table: "Schools",
                type: "TEXT",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Province",
                table: "Schools",
                type: "TEXT",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CIF",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "City",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "PostalCode",
                table: "Schools");

            migrationBuilder.DropColumn(
                name: "Province",
                table: "Schools");
        }
    }
}
