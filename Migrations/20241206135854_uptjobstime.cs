using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace babiltr.Migrations
{
    /// <inheritdoc />
    public partial class uptjobstime : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimeRemaining",
                table: "Jobs",
                newName: "TimerStartTime");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "TimerStartTime",
                table: "Jobs",
                newName: "TimeRemaining");
        }
    }
}
