using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babiltr.Migrations
{
    /// <inheritdoc />
    public partial class tablesupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_AspNetUsers_UserID",
                table: "Applications");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "",
                oldClrType: typeof(string),
                oldType: "nvarchar(max)",
                oldNullable: true);

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

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_AspNetUsers_UserID",
                table: "Applications",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Applications_AspNetUsers_HiredUserID",
                table: "Applications");

            migrationBuilder.DropForeignKey(
                name: "FK_Applications_AspNetUsers_UserID",
                table: "Applications");

            migrationBuilder.DropIndex(
                name: "IX_Applications_HiredUserID",
                table: "Applications");

            migrationBuilder.DropColumn(
                name: "HiredUserID",
                table: "Applications");

            migrationBuilder.AlterColumn<string>(
                name: "Status",
                table: "Jobs",
                type: "nvarchar(max)",
                nullable: true,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AddForeignKey(
                name: "FK_Applications_AspNetUsers_UserID",
                table: "Applications",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
