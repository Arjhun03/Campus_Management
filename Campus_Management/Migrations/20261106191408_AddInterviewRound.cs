using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campus_Management.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewRound : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterviewRounds",
                columns: table => new
                {
                    RoundId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    DriveId = table.Column<int>(type: "int", nullable: false),
                    RoundNumber = table.Column<int>(type: "int", nullable: false),
                    RoundName = table.Column<string>(type: "nvarchar(100)", maxLength: 100, nullable: false),
                    RoundType = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Description = table.Column<string>(type: "nvarchar(1000)", maxLength: 1000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewRounds", x => x.RoundId);
                    table.ForeignKey(
                        name: "FK_InterviewRounds_RecruitmentDrives_DriveId",
                        column: x => x.DriveId,
                        principalTable: "RecruitmentDrives",
                        principalColumn: "DriveId",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRounds_DriveId",
                table: "InterviewRounds",
                column: "DriveId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewRounds_DriveId_RoundNumber",
                table: "InterviewRounds",
                columns: new[] { "DriveId", "RoundNumber" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewRounds");
        }
    }
}
