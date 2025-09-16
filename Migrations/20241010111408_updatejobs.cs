using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babiltr.Migrations
{
    /// <inheritdoc />
    public partial class updatejobs : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "Requirements",
                table: "Jobs",
                newName: "Skills");

            migrationBuilder.RenameColumn(
                name: "Location",
                table: "Jobs",
                newName: "JobType");

            migrationBuilder.AddColumn<string>(
                name: "City",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Country",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "Experience",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<string>(
                name: "JobCategories",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "City",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Country",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "Experience",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "JobCategories",
                table: "Jobs");

            migrationBuilder.RenameColumn(
                name: "Skills",
                table: "Jobs",
                newName: "Requirements");

            migrationBuilder.RenameColumn(
                name: "JobType",
                table: "Jobs",
                newName: "Location");
        }
    }
}
