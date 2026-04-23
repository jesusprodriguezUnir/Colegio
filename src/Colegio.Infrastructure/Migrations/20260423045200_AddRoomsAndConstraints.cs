using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Colegio.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class AddRoomsAndConstraints : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Level",
                table: "TeacherAvailabilities",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<string>(
                name: "Color",
                table: "Subjects",
                type: "TEXT",
                maxLength: 20,
                nullable: false,
                defaultValue: "#6366f1");

            migrationBuilder.AddColumn<Guid>(
                name: "RequiredRoomId",
                table: "Subjects",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<Guid>(
                name: "RoomId",
                table: "Schedules",
                type: "TEXT",
                nullable: true);

            migrationBuilder.AddColumn<int>(
                name: "Type",
                table: "Schedules",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.CreateTable(
                name: "Rooms",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Name = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Capacity = table.Column<int>(type: "INTEGER", nullable: false),
                    Building = table.Column<string>(type: "TEXT", maxLength: 100, nullable: true),
                    Floor = table.Column<int>(type: "INTEGER", nullable: true),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Rooms", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ScheduleConstraints",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    Type = table.Column<int>(type: "INTEGER", nullable: false),
                    Hardness = table.Column<int>(type: "INTEGER", nullable: false),
                    Weight = table.Column<int>(type: "INTEGER", nullable: false, defaultValue: 5),
                    TeacherId = table.Column<Guid>(type: "TEXT", nullable: true),
                    SubjectId = table.Column<Guid>(type: "TEXT", nullable: true),
                    ClassroomId = table.Column<Guid>(type: "TEXT", nullable: true),
                    Parameters = table.Column<string>(type: "TEXT", maxLength: 1000, nullable: true),
                    IsActive = table.Column<bool>(type: "INTEGER", nullable: false),
                    Description = table.Column<string>(type: "TEXT", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ScheduleConstraints", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ScheduleConstraints_Classrooms_ClassroomId",
                        column: x => x.ClassroomId,
                        principalTable: "Classrooms",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleConstraints_Subjects_SubjectId",
                        column: x => x.SubjectId,
                        principalTable: "Subjects",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_ScheduleConstraints_Teachers_TeacherId",
                        column: x => x.TeacherId,
                        principalTable: "Teachers",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_Subjects_RequiredRoomId",
                table: "Subjects",
                column: "RequiredRoomId");

            migrationBuilder.CreateIndex(
                name: "IX_Schedules_RoomId",
                table: "Schedules",
                column: "RoomId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleConstraints_ClassroomId",
                table: "ScheduleConstraints",
                column: "ClassroomId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleConstraints_SubjectId",
                table: "ScheduleConstraints",
                column: "SubjectId");

            migrationBuilder.CreateIndex(
                name: "IX_ScheduleConstraints_TeacherId",
                table: "ScheduleConstraints",
                column: "TeacherId");

            migrationBuilder.AddForeignKey(
                name: "FK_Schedules_Rooms_RoomId",
                table: "Schedules",
                column: "RoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);

            migrationBuilder.AddForeignKey(
                name: "FK_Subjects_Rooms_RequiredRoomId",
                table: "Subjects",
                column: "RequiredRoomId",
                principalTable: "Rooms",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Schedules_Rooms_RoomId",
                table: "Schedules");

            migrationBuilder.DropForeignKey(
                name: "FK_Subjects_Rooms_RequiredRoomId",
                table: "Subjects");

            migrationBuilder.DropTable(
                name: "Rooms");

            migrationBuilder.DropTable(
                name: "ScheduleConstraints");

            migrationBuilder.DropIndex(
                name: "IX_Subjects_RequiredRoomId",
                table: "Subjects");

            migrationBuilder.DropIndex(
                name: "IX_Schedules_RoomId",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "Level",
                table: "TeacherAvailabilities");

            migrationBuilder.DropColumn(
                name: "Color",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "RequiredRoomId",
                table: "Subjects");

            migrationBuilder.DropColumn(
                name: "RoomId",
                table: "Schedules");

            migrationBuilder.DropColumn(
                name: "Type",
                table: "Schedules");
        }
    }
}
