using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Colegio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class Phase4TeacherPreferences : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "MaxGapsPerDay",
                table: "Teachers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 1);

            migrationBuilder.AddColumn<int>(
                name: "MinDailyHours",
                table: "Teachers",
                type: "INTEGER",
                nullable: false,
                defaultValue: 2);

            migrationBuilder.AddColumn<bool>(
                name: "PreferCompactSchedule",
                table: "Teachers",
                type: "INTEGER",
                nullable: false,
                defaultValue: true);

            migrationBuilder.AddColumn<int>(
                name: "PreferredFreeDay",
                table: "Teachers",
                type: "INTEGER",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "AnonymousGroup",
                table: "Rooms",
                type: "TEXT",
                maxLength: 100,
                nullable: true);

            migrationBuilder.CreateTable(
                name: "ClassUnits",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ClassroomId = table.Column<Guid>(type: "TEXT", nullable: false),
                    SubjectId = table.Column<Guid>(type: "TEXT", nullable: false),
                    TeacherId = table.Column<Guid>(type: "TEXT", nullable: true),
                    WeeklySessions = table.Column<int>(type: "INTEGER", nullable: false),
                    SessionDuration = table.Column<int>(type: "INTEGER", nullable: false),
                    AllowConsecutiveDays = table.Column<bool>(type: "INTEGER", nullable: false),
                    PreferNonConsecutive = table.Column<bool>(type: "INTEGER", nullable: false),
                    AllowDoubleSession = table.Column<bool>(type: "INTEGER", nullable: false),
                    MaxSessionsPerDay = table.Column<int>(type: "INTEGER", nullable: false),
                    PreferredRoomId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SimultaneousGroupId = table.Column<Guid>(type: "TEXT", nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClassUnits", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClassUnits_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassUnits_Rooms_PreferredRoomId",
                        column: x => x.PreferredRoomId,
                        principalTable: "Rooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                    table.ForeignKey(
                        name: "FK_ClassUnits_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ClassUnits_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.SetNull);
                });

            migrationBuilder.CreateIndex(
                name: "IX_ClassUnits_ClassroomId",
                table: "ClassUnits",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassUnits_PreferredRoomId",
                table: "ClassUnits",
                column: "PreferredRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassUnits_SubjectId",
                table: "ClassUnits",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ClassUnits_TeacherId",
                table: "ClassUnits",
                column: "TeacherId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "ClassUnits");

            migrationBuilder.DropColumn(
                name: "MaxGapsPerDay",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "MinDailyHours",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "PreferCompactSchedule",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "PreferredFreeDay",
                table: "Teachers");

            migrationBuilder.DropColumn(
                name: "AnonymousGroup",
                table: "Rooms");
        }
    }
}
