using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CBAI.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipApplicationWorkflow : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Members",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApplicationUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    MembershipTypeName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    ShareholderSince = table.Column<DateOnly>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Members", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MembershipApplications",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    ApplicantUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    RequestedMembershipTypeName = table.Column<string>(type: "TEXT", maxLength: 100, nullable: false),
                    SponsorUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    Status = table.Column<string>(type: "TEXT", nullable: false),
                    DecisionNotes = table.Column<string>(type: "TEXT", nullable: true),
                    DecidedByUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: true),
                    CreatedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    SubmittedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true),
                    DecidedAtUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipApplications", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "MembershipApplicationAuditEntries",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "TEXT", nullable: false),
                    MembershipApplicationId = table.Column<Guid>(type: "TEXT", nullable: false),
                    Action = table.Column<string>(type: "TEXT", nullable: false),
                    PerformedByUserId = table.Column<string>(type: "TEXT", maxLength: 450, nullable: false),
                    TimestampUtc = table.Column<DateTimeOffset>(type: "TEXT", nullable: false),
                    Details = table.Column<string>(type: "TEXT", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MembershipApplicationAuditEntries", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MembershipApplicationAuditEntries_MembershipApplications_MembershipApplicationId",
                        column: x => x.MembershipApplicationId,
                        principalTable: "MembershipApplications",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateIndex(
                name: "IX_MembershipApplicationAuditEntries_MembershipApplicationId",
                table: "MembershipApplicationAuditEntries",
                column: "MembershipApplicationId");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "Members");

            migrationBuilder.DropTable(
                name: "MembershipApplicationAuditEntries");

            migrationBuilder.DropTable(
                name: "MembershipApplications");
        }
    }
}
