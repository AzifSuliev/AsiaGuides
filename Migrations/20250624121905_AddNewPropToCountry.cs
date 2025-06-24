using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace AsiaGuides.Migrations
{
    /// <inheritdoc />
    public partial class AddNewPropToCountry : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ImagePublicId",
                table: "Countries",
                type: "text",
                nullable: true);

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 1,
                column: "ImagePublicId",
                value: null);

            migrationBuilder.UpdateData(
                table: "Countries",
                keyColumn: "Id",
                keyValue: 2,
                column: "ImagePublicId",
                value: null);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ImagePublicId",
                table: "Countries");
        }
    }
}
