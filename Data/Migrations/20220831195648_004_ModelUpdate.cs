using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace BugTracker.data.migrations
{
    public partial class _004_ModelUpdate : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.RenameColumn(
                name: "ImageFile",
                table: "Projects",
                newName: "ImageFileData");

            migrationBuilder.AddColumn<string>(
                name: "ImageFileName",
                table: "Projects",
                type: "text",
                nullable: true);
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImageFileName",
                table: "Projects");

            migrationBuilder.RenameColumn(
                name: "ImageFileData",
                table: "Projects",
                newName: "ImageFile");
        }
    }
}
