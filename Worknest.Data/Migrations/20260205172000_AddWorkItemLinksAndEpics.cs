using System;
using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Worknest.Data.Migrations
{
    /// <inheritdoc />
    public partial class AddWorkItemLinksAndEpics : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<Guid>(
                name: "EpicId",
                table: "WorkItems",
                type: "uniqueidentifier",
                nullable: true);

            migrationBuilder.CreateTable(
                name: "WorkItemLinks",
                columns: table => new
                {
                    Id = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    Type = table.Column<string>(type: "nvarchar(max)", nullable: false),
                    SourceId = table.Column<Guid>(type: "uniqueidentifier", nullable: false),
                    TargetId = table.Column<Guid>(type: "uniqueidentifier", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_WorkItemLinks", x => x.Id);
                    table.ForeignKey(
                        name: "FK_WorkItemLinks_WorkItems_SourceId",
                        column: x => x.SourceId,
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_WorkItemLinks_WorkItems_TargetId",
                        column: x => x.TargetId,
                        principalTable: "WorkItems",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_EpicId",
                table: "WorkItems",
                column: "EpicId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemLinks_SourceId",
                table: "WorkItemLinks",
                column: "SourceId");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemLinks_TargetId",
                table: "WorkItemLinks",
                column: "TargetId");

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItems_WorkItems_EpicId",
                table: "WorkItems",
                column: "EpicId",
                principalTable: "WorkItems",
                principalColumn: "Id",
                onDelete: ReferentialAction.Restrict);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkItems_WorkItems_EpicId",
                table: "WorkItems");

            migrationBuilder.DropTable(
                name: "WorkItemLinks");

            migrationBuilder.DropIndex(
                name: "IX_WorkItems_EpicId",
                table: "WorkItems");

            migrationBuilder.DropColumn(
                name: "EpicId",
                table: "WorkItems");
        }
    }
}
