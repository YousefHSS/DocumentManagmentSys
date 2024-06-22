using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoucmentManagmentSys.Migrations
{
    /// <inheritdoc />
    public partial class ColoredVars : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.AddColumn<string>(
                name: "ColoredVariables",
                table: "DocumentTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "[]");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "ColoredVariables",
                table: "DocumentTemplates");
        }
    }
}
