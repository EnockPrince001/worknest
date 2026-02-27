using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Worknest.Data.Migrations
{
    /// <inheritdoc />
    public partial class FixJobTitleMismatch : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<int>(
                name: "Order",
                table: "WorkItems",
                type: "int",
                nullable: false,
                defaultValue: 0);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Order",
                table: "WorkItems");
        }
    }
}
