using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babiltr.Migrations
{
    /// <inheritdoc />
    public partial class educaitonupdate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Education_AspNetUsers_UserID",
                table: "Education");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Education",
                table: "Education");

            migrationBuilder.DropColumn(
                name: "Degree",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "FieldOfStudy",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "SchoolName",
                table: "AspNetUsers");

            migrationBuilder.DropColumn(
                name: "Years",
                table: "AspNetUsers");

            migrationBuilder.RenameTable(
                name: "Education",
                newName: "Educations");

            migrationBuilder.RenameIndex(
                name: "IX_Education_UserID",
                table: "Educations",
                newName: "IX_Educations_UserID");

            migrationBuilder.AddPrimaryKey(
                name: "PK_Educations",
                table: "Educations",
                column: "EducationID");

            migrationBuilder.AddForeignKey(
                name: "FK_Educations_AspNetUsers_UserID",
                table: "Educations",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Educations_AspNetUsers_UserID",
                table: "Educations");

            migrationBuilder.DropPrimaryKey(
                name: "PK_Educations",
                table: "Educations");

            migrationBuilder.RenameTable(
                name: "Educations",
                newName: "Education");

            migrationBuilder.RenameIndex(
                name: "IX_Educations_UserID",
                table: "Education",
                newName: "IX_Education_UserID");

            migrationBuilder.AddColumn<string>(
                name: "Degree",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "FieldOfStudy",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "SchoolName",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "Years",
                table: "AspNetUsers",
                type: "nvarchar(max)",
                nullable: true);

            migrationBuilder.AddPrimaryKey(
                name: "PK_Education",
                table: "Education",
                column: "EducationID");

            migrationBuilder.AddForeignKey(
                name: "FK_Education_AspNetUsers_UserID",
                table: "Education",
                column: "UserID",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }
    }
}
