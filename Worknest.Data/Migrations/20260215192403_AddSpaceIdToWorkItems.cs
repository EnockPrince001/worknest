using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Worknest.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddSpaceIdToWorkItems : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "SpaceId",
                table: "WorkItems",
                type: "uniqueidentifier",
                nullable: true,
                defaultValue: new Guid("00000000-0000-0000-0000-000000000000"));

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_SpaceId",
                table: "WorkItems",
                column: "SpaceId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_Spaces_SpaceId",
                table: "WorkItems",
                column: "SpaceId",
                principalTable: "Spaces",
                principalColumn: "Id",
                onDelete: ReferentialAction.Cascade);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_Spaces_SpaceId",
                table: "WorkItems");

            migrationBuilder.DropIndex(
                name: "IX_WorkItems_SpaceId",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "SpaceId",
                table: "WorkItems");
        }
    }
}
