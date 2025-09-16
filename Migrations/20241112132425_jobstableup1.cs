using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babiltr.Migrations
{
    /// <inheritdoc />
    public partial class jobstableup1 : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<bool>(
                name: "PaymentCompleted",
                table: "Jobs",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PaymentCompleted",
                table: "Jobs");
        }
    }
}
