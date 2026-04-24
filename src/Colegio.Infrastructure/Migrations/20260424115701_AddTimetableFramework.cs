using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Colegio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddTimetableFramework : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "TimetableFrameworkId",
                table: "TimeSlots",
                type: "TEXT",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "TimetableFrameworks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 200, nullable: false),
                    SessionType = table.Column<int>(type: "INTEGER", nullable: false),
                    HasAfternoon = table.Column<bool>(type: "INTEGER", nullable: false),
                    MorningStart = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    MorningEnd = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    AfternoonStart = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    AfternoonEnd = table.Column<TimeSpan>(type: "TEXT", nullable: true),
                    SessionDurationMinutes = table.Column<int>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_TimetableFrameworks", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "BreakDefinitions",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    TimetableFrameworkId = table.Column<Guid>(type: "TEXT", nullable: false),
                    StartTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    EndTime = table.Column<TimeSpan>(type: "TEXT", nullable: false),
                    Label = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_BreakDefinitions", x => x.Id);
                    table.ForeignKey(
                        name: "FK_BreakDefinitions_TimetableFrameworks_TimetableFrameworkId",
                        column: x => x.TimetableFrameworkId,
                        principalTable: "TimetableFrameworks",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_TimeSlots_TimetableFrameworkId",
                table: "TimeSlots",
                column: "TimetableFrameworkId");

            migrationBuilder.CreateIndex(
                name: "IX_BreakDefinitions_TimetableFrameworkId",
                table: "BreakDefinitions",
                column: "TimetableFrameworkId");

            migrationBuilder.AddForeignKey(
                name: "FK_TimeSlots_TimetableFrameworks_TimetableFrameworkId",
                table: "TimeSlots",
                column: "TimetableFrameworkId",
                principalTable: "TimetableFrameworks",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_TimeSlots_TimetableFrameworks_TimetableFrameworkId",
                table: "TimeSlots");

            migrationBuilder.DropTable(
                name: "BreakDefinitions");

            migrationBuilder.DropTable(
                name: "TimetableFrameworks");

            migrationBuilder.DropIndex(
                name: "IX_TimeSlots_TimetableFrameworkId",
                table: "TimeSlots");

            migrationBuilder.DropColumn(
                name: "TimetableFrameworkId",
                table: "TimeSlots");
        }
    }
}
