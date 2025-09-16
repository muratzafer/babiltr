using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babiltr.Migrations
{
    /// <inheritdoc />
    public partial class updatechatstable : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeletedId",
                table: "Messages");

            migrationBuilder.AddColumn<int>(
                name: "DeleteId",
                table: "Chats",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "DeleteId",
                table: "Chats");

            migrationBuilder.AddColumn<int>(
                name: "DeletedId",
                table: "Messages",
                type: "int",
                nullable: true);
        }
    }
}
