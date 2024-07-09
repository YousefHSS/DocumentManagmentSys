using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace DoucmentManagmentSys.Migrations
{
    /// <inheritdoc />
    public partial class DocumentTemplatesTBH : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "CreatedBy",
                table: "DocumentTemplates");

            migrationBuilder.AddColumn<string>(
                name: "Discriminator",
                table: "DocumentTemplates",
                type: "nvarchar(55)",
                maxLength: 55,
                nullable: false,
                defaultValue: "");
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropColumn(
                name: "Discriminator",
                table: "DocumentTemplates");

            migrationBuilder.AddColumn<string>(
                name: "CreatedBy",
                table: "DocumentTemplates",
                type: "nvarchar(max)",
                nullable: false,
                defaultValue: "");
        }
    }
}
