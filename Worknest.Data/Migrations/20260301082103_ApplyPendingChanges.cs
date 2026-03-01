using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Worknest.Data.Migrations
{
    /// <inheritdoc />
    public partial class ApplyPendingChanges : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_Activities_WorkItems_WorkItemId1",
                table: "Activities");

            migrationBuilder.DropIndex(
                name: "IX_Activities_WorkItemId1",
                table: "Activities");

            migrationBuilder.DropColumn(
                name: "WorkItemId1",
                table: "Activities");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "WorkItemId1",
                table: "Activities",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateIndex(
                name: "IX_Activities_WorkItemId1",
                table: "Activities",
                column: "WorkItemId1");

            migrationBuilder.AddForeignKey(
                name: "FK_Activities_WorkItems_WorkItemId1",
                table: "Activities",
                column: "WorkItemId1",
                principalTable: "WorkItems",
                principalColumn: "Id");
        }
    }
}
