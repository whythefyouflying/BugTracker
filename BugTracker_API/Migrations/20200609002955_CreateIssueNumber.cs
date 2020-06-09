using Microsoft.EntityFrameworkCore.Migrations;

namespace BugTracker_API.Migrations
{
    public partial class CreateIssueNumber : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Number",
                table: "Issues",
                nullable: false,
                defaultValue: 0);

            migrationBuilder.AddColumn<bool>(
                name: "isDeleted",
                table: "Issues",
                nullable: false,
                defaultValue: false);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Number",
                table: "Issues");

            migrationBuilder.DropColumn(
                name: "isDeleted",
                table: "Issues");
        }
    }
}
