using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Campus_Management.Migrations
{
    /// <inheritdoc />
    public partial class AddRecruitmentDrive : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "RecruitmentDrives",
                columns: table => new
                {
                    DriveId = table.Column<int>(type: "int", nullable: false)
                        .Annotation("SqlServer:Identity", "1, 1"),
                    JobId = table.Column<int>(type: "int", nullable: false),
                    CompanyId = table.Column<int>(type: "int", nullable: false),
                    DriveName = table.Column<string>(type: "nvarchar(200)", maxLength: 200, nullable: false),
                    DriveDate = table.Column<DateTime>(type: "datetime2", nullable: false),
                    EndDate = table.Column<DateTime>(type: "datetime2", nullable: true),
                    Venue = table.Column<string>(type: "nvarchar(250)", maxLength: 250, nullable: false),
                    DriveMode = table.Column<string>(type: "nvarchar(50)", maxLength: 50, nullable: false),
                    Status = table.Column<int>(type: "int", nullable: false),
                    Instructions = table.Column<string>(type: "nvarchar(2000)", maxLength: 2000, nullable: true),
                    CreatedAt = table.Column<DateTime>(type: "datetime2", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_RecruitmentDrives", x => x.DriveId);
                    table.ForeignKey(
                        name: "FK_RecruitmentDrives_Companies_CompanyId",
                        column: x => x.CompanyId,
                        principalTable: "Companies",
                        principalColumn: "CompanyId",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_RecruitmentDrives_JobOpenings_JobId",
                        column: x => x.JobId,
                        principalTable: "JobOpenings",
                        principalColumn: "JobId",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentDrives_CompanyId",
                table: "RecruitmentDrives",
                column: "CompanyId");

            migrationBuilder.CreateIndex(
                name: "IX_RecruitmentDrives_JobId",
                table: "RecruitmentDrives",
                column: "JobId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "RecruitmentDrives");
        }
    }
}
