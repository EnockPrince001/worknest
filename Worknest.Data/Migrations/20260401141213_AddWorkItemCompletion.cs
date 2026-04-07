using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Worknest.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkItemCompletion : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<DateTime>(
                name: "CompletedAt",
                table: "WorkItems",
                type: "datetime2",
                nullable: true);

            migrationBuilder.AddColumn<bool>(
                name: "IsCompleted",
                table: "WorkItems",
                type: "bit",
                nullable: false,
                defaultValue: false);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CompletedAt",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "IsCompleted",
                table: "WorkItems");
        }
    }
}