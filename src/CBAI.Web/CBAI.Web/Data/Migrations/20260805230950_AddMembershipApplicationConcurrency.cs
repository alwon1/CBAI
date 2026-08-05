using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace CBAI.Web.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddMembershipApplicationConcurrency : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Version",
                table: "MembershipApplications",
                type: "INTEGER",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Version",
                table: "MembershipApplications");
        }
    }
}
