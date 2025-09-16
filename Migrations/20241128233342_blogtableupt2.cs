using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babiltr.Migrations
{
    /// <inheritdoc />
    public partial class blogtableupt2 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "Author",
                table: "Blogs",
                type: "nvarchar(max)",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Author",
                table: "Blogs");
        }
    }
}
