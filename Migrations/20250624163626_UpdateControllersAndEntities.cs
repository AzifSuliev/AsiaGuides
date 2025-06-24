using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AsiaGuides.Migrations
{
    /// <inheritdoc />
    public partial class UpdateControllersAndEntities : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "Cities",
                type: "text",
                nullable: true);

            migrationBuilder.AddColumn<string>(
                name: "PublicId",
                table: "AttractionImage",
                type: "text",
                nullable: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "Cities");

            migrationBuilder.DropColumn(
                name: "PublicId",
                table: "AttractionImage");
        }
    }
}
