using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace Worknest.Data.Migrations
{
    /// <inheritdoc />
    public partial class AlignSpaceWorkItemFlowAndComments : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "WorkItems",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "Spaces",
                type: "nvarchar(450)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(max)");

            migrationBuilder.CreateIndex(
                name: "IX_WorkItems_Key",
                table: "WorkItems",
                column: "Key",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_WorkItemComments_CreatedBy",
                table: "WorkItemComments",
                column: "CreatedBy");

            migrationBuilder.CreateIndex(
                name: "IX_Spaces_Key",
                table: "Spaces",
                column: "Key",
                unique: true);

            migrationBuilder.AddForeignKey(
                name: "FK_WorkItemComments_AspNetUsers_CreatedBy",
                table: "WorkItemComments",
                column: "CreatedBy",
                principalTable: "AspNetUsers",
                principalColumn: "Id",
                onDelete: ReferentialAction.SetNull);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropForeignKey(
                name: "FK_WorkItemComments_AspNetUsers_CreatedBy",
                table: "WorkItemComments");

            migrationBuilder.DropIndex(
                name: "IX_WorkItems_Key",
                table: "WorkItems");

            migrationBuilder.DropIndex(
                name: "IX_WorkItemComments_CreatedBy",
                table: "WorkItemComments");

            migrationBuilder.DropIndex(
                name: "IX_Spaces_Key",
                table: "Spaces");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "WorkItems",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");

            migrationBuilder.AlterColumn<string>(
                name: "Key",
                table: "Spaces",
                type: "nvarchar(max)",
                nullable: false,
                oldClrType: typeof(string),
                oldType: "nvarchar(450)");
        }
    }
}
