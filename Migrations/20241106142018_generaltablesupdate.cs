using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babiltr.Migrations
{
    /// <inheritdoc />
    public partial class generaltablesupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_AspNetUsers_HiredUserID",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_HiredUserID",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "JobCategories",
                table: "Jobs");

            migrationBuilder.DropColumn(
                name: "HiredUserID",
                table: "Applications");

            migrationBuilder.CreateIndex(
                name: "IX_Applications_JobID",
                table: "Applications",
                column: "JobID");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_Jobs_JobID",
                table: "Applications",
                column: "JobID",
                principalTable: "Jobs",
                principalColumn: "JobID",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_Jobs_JobID",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_JobID",
                table: "Applications");

            migrationBuilder.AddColumn<string>(
                name: "JobCategories",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");

            migrationBuilder.AddColumn<int>(
                name: "HiredUserID",
                table: "Applications",
                type: "int",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Applications_HiredUserID",
                table: "Applications",
                column: "HiredUserID");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_AspNetUsers_HiredUserID",
                table: "Applications",
                column: "HiredUserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }
    }
}
