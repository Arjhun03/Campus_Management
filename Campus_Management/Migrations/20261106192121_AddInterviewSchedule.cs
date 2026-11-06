using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campus_Management.Migrations
{
    /// <inheritdoc />
    public partial class AddInterviewSchedule : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "InterviewSchedules",
                columns: table => new
                {
                    ScheduleId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    RoundId = table.Column<int>(type: "int", nullable: false),
                    ApplicationId = table.Column<int>(type: "int", nullable: false),
                    ScheduledStartTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    ScheduledEndTime = table.Column<DateTime>(type: "datetime2", nullable: false),
                    InterviewerName = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: false),
                    InterviewerEmail = table.Column<string>(type: "nvarchar(150)", maxLength: 150, nullable: true),
                    MeetingLinkOrLocation = table.Column<string>(type: "nvarchar(500)", maxLength: 500, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Feedback = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    Score = table.Column<decimal>(type: "decimal(5,2)", nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false),
                    UpdatedAt = table.Column<DateTime>(type: "datetime2", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_InterviewSchedules", x => x.ScheduleId);
                    table.ForeignKey(
                        name: "FK_InterviewSchedules_InterviewRounds_RoundId",
                        column: x => x.RoundId,
                        principalTable: "InterviewRounds",
                        principalColumn: "RoundId",
                        onDelete: ReferentialAction.Cascade);
                    table.ForeignKey(
                        name: "FK_InterviewSchedules_JobApplications_ApplicationId",
                        column: x => x.ApplicationId,
                        principalTable: "JobApplications",
                        principalColumn: "ApplicationId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSchedules_ApplicationId",
                table: "InterviewSchedules",
                column: "ApplicationId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSchedules_RoundId",
                table: "InterviewSchedules",
                column: "RoundId");

            migrationBuilder.CreateIndex(
                name: "IX_InterviewSchedules_RoundId_ApplicationId",
                table: "InterviewSchedules",
                columns: new[] { "RoundId", "ApplicationId" },
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "InterviewSchedules");
        }
    }
}
